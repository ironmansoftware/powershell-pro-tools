using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Windows.Forms;
using PowerShellToolsPro.LanguageService;
using CodeNamespace = System.CodeDom.CodeNamespace;
using System.Globalization;
using System.Xml.Linq;
using System.Threading;
using IM.CodeDom;
using IMS.FormDesigner;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerShellToolsPro.FormsDesigner
{
    public abstract class PowerShellCodeDomProvider : CodeDomProvider
    {
        protected abstract void Log(string message);

        public abstract string GetDesignerFileName(TextReader codeStream);
        public abstract string GetDesignerFileName(TextWriter codeStream);

        public abstract string GetCodeFileName(TextReader codeStream);
        public abstract string GetCodeFileName(TextWriter codeStream);

        public abstract void InsertIntoBeginningOfFile(string fileName, string text);

        private readonly EventGenerationType _eventGenerationType;

        public PowerShellCodeDomProvider(EventGenerationType eventGenerationType)
	    {
            _eventGenerationType = eventGenerationType;
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public override ICodeGenerator CreateGenerator()
        {
            return new PowerShellCodeGenerator(this, _eventGenerationType);
        }

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public override ICodeCompiler CreateCompiler()
        {
            throw new NotImplementedException();
        }

        public override CodeCompileUnit Parse(TextReader codeStream)
        {
            var designerFileName = GetDesignerFileName(codeStream);
            using (var textReader = new StreamReader(designerFileName))
            {
                var script = textReader.ReadToEnd();

                Ast ast;
                try
                {
                    ast = ScriptBlock.Create(script).Ast;
                }
                catch
                {
                    throw new Exception("Failed to parse designer file.");
                }

                var visitor = new CodeDomAstVisitor();
                var unit = ast.Visit(visitor) as CodeCompileUnit;

                unit.ReferencedAssemblies.Add(typeof(Point).Assembly.FullName);
                unit.ReferencedAssemblies.Add(typeof(Form).Assembly.FullName);

                Debug.WriteLine("Generated Unit");

    #if DEBUG
                var generator = new DebugCodeGenerator();
                generator.GenerateCodeFromCompileUnit(unit, null, null);
    #endif

                return unit;
            }
        }
    }

    public enum EventGenerationType
    {
        Variable,
        Function
    }

    public class PowerShellCodeGenerator : ICodeGenerator
    {
        public TypeModel _currentType;
        private readonly PowerShellCodeDomProvider powerShellCodeDomProvider;
        private string _ps1FileName;
        private readonly EventGenerationType _eventGenerationType;

        public PowerShellCodeGenerator(PowerShellCodeDomProvider powerShellCodeDomProvider, EventGenerationType eventGenerationType)
        {
            this.powerShellCodeDomProvider = powerShellCodeDomProvider;
            _eventGenerationType = eventGenerationType;
        }

        public bool IsValidIdentifier(string value)
        {
            return true;
        }

        public void ValidateIdentifier(string value)
        {

        }

        public string CreateEscapedIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public string CreateValidIdentifier(string value)
        {
            //Here have some documentation:
            //CreateValidIdentifier tests whether the identifier conflicts with reserved or language keywords, and returns a valid identifier name that does not conflict. The returned identifier will contain the same value but, if it conflicts with reserved or language keywords, will have escape code formatting added to differentiate the identifier from the keyword. Typically, if the value needs modification, value is returned preceded by an underscore "_".

            return value;
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            throw new NotImplementedException();
        }

        public bool Supports(GeneratorSupport supports)
        {
            return true;
        }

        public void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
	        if (e is CodeCastExpression)
	        {
		        GenerateCodeFromCodeCastExpression(e as CodeCastExpression, w, o);
	        }

            if (e is CodeObjectCreateExpression)
            {
                GenerateCodeFromObjectCreateExpression(e as CodeObjectCreateExpression, w, o);
            }

            if (e is CodePrimitiveExpression)
            {
                GenerateCodeFromPrimitiveExpression(e as CodePrimitiveExpression, w, o);
            }

            if (e is CodeVariableReferenceExpression)
            {
                GenerateCodeFromVariableReferenceExpression(e as CodeVariableReferenceExpression, w, o);
            }

            if (e is CodePropertyReferenceExpression)
            {
                GenerateCodeFromPropertyReferenceExpression(e as CodePropertyReferenceExpression, w, o);
            }

            if (e is CodeFieldReferenceExpression)
            {
                GenerateCodeFromFieldReferenceExpression(e as CodeFieldReferenceExpression, w, o);
            }

            if (e is CodeThisReferenceExpression)
            {
                GenerateCodeFromThisReferenceExpression(e as CodeThisReferenceExpression, w, o);
            }

            if (e is CodeMethodInvokeExpression)
            {
                GenerateCodeFromMethodInvokeExpression(e as CodeMethodInvokeExpression, w, o);
            }

            if (e is CodeDelegateCreateExpression)
            {
                GenerateCodeFromDelegateCreateExpression(e as CodeDelegateCreateExpression, w);
            }

            if (e is CodeTypeReferenceExpression)
            {
                GenerateCodeFromTypeReferenceExpression(e as CodeTypeReferenceExpression, w);
            }

			if (e is CodeArrayCreateExpression)
			{
				GenerateCodeFromArrayCreateExpression(e as CodeArrayCreateExpression, w, o);
			}

            if (e is CodeBinaryOperatorExpression)
            {
                GenerateCodeFromBinaryOperatorExpression(e as CodeBinaryOperatorExpression, w, o);
            }
        }

        private void GenerateCodeFromBinaryOperatorExpression(CodeBinaryOperatorExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(e.Left, w, o);

            switch (e.Operator)
            {
                case CodeBinaryOperatorType.BitwiseOr:
                    w.Write(" -bor ");
                    break;

                case CodeBinaryOperatorType.BitwiseAnd:
                    w.Write(" -band ");
                    break;

                default:
                    break;
            }

            GenerateCodeFromExpression(e.Right, w, o);
        }


        private void GenerateCodeFromArrayCreateExpression(CodeArrayCreateExpression e, TextWriter w, CodeGeneratorOptions o)
		{
			w.Write("[{0}[]]", e.CreateType.BaseType);

			w.Write("@(");

			int count = e.Initializers.Count;

			foreach (CodeExpression expression in e.Initializers)
			{
				count--;
				GenerateCodeFromExpression(expression, w, null);
				if (count > 0)
				{
					w.Write(",");
				}
			}

			w.Write(")");
		}

		private void GenerateCodeFromTypeReferenceExpression(CodeTypeReferenceExpression e, TextWriter w)
        {
            w.Write("[" + e.Type.BaseType + "]");
        }

        private void GenerateCodeFromDelegateCreateExpression(CodeDelegateCreateExpression e, TextWriter w)
        {
            var methodName = Regex.Replace(e.MethodName, "[^a-zA-Z0-9_]", string.Empty);

            if (_eventGenerationType == EventGenerationType.Function)
            {
                w.Write($"{{ {methodName} }}");

                if (powerShellCodeDomProvider is CommandLineCodeDomProvider)
                {
                    var ast = Parser.ParseFile(_ps1FileName, out Token[] tokens, out ParseError[] parseError);
                    var assignment = ast.Find(m => m is FunctionDefinitionAst fda && fda.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase), true);
                    if (assignment == null)
                    {
                        var contents = File.ReadAllText(_ps1FileName);
                        contents = $"function {methodName} {{{Environment.NewLine}}}{Environment.NewLine}{contents}";
                        File.WriteAllText(_ps1FileName, contents);
                    }
                }
            }
            else
            {
                w.Write("$" + methodName);

                if (powerShellCodeDomProvider is CommandLineCodeDomProvider)
                {
                    var ast = Parser.ParseFile(_ps1FileName, out Token[] tokens, out ParseError[] parseError);
                    var assignment = ast.Find(m => m is AssignmentStatementAst asa && asa.Left is VariableExpressionAst vea && vea.VariablePath.UserPath.Equals(methodName, StringComparison.OrdinalIgnoreCase), true);
                    if (assignment == null)
                    {
                        var contents = File.ReadAllText(_ps1FileName);
                        contents = $"${methodName} = {{{Environment.NewLine}}}{Environment.NewLine}{contents}";
                        File.WriteAllText(_ps1FileName, contents);
                    }
                }
            }
        }

        private void GenerateCodeFromMethodInvokeExpression(CodeMethodInvokeExpression e, TextWriter w,
            CodeGeneratorOptions o)
        {
            var needsParameters = false;
            if (e.Method.TargetObject is CodeThisReferenceExpression)
            {
                if (_currentType.Methods.All(m => m.Name != e.Method.MethodName))
                {
                    w.Write("${0}.", _currentType.Name);
                    needsParameters = true;
                }
                else
                {
                    w.Write(". ");
                }
            }
            else
            {
				var varRef = e.Method.TargetObject as CodeVariableReferenceExpression;
				if (varRef != null && varRef.VariableName.Equals("resources") && e.Method.MethodName.Equals("GetObject"))
				{
                    var resourceName = e.Parameters[0] as CodePrimitiveExpression;

                    w.Write("$resources.'");
                    w.Write(resourceName.Value);
                    w.Write("'");

					return;
				}

                if (varRef != null && varRef.VariableName.Equals("resources") && e.Method.MethodName.Equals("GetString"))
                {
                    var resourceName = e.Parameters[0] as CodePrimitiveExpression;

                    w.Write("[System.String]$resources.'");
                    w.Write(resourceName.Value);
                    w.Write("'");

                    return;
                }

                GenerateCodeFromExpression(e.Method.TargetObject, w, o);
	            if (e.Method.TargetObject is CodeTypeReferenceExpression)
	            {
		            w.Write("::");
				}
	            else
	            {
		            w.Write(".");
				}

                needsParameters = true;
            }

            w.Write(e.Method.MethodName);

            int count = e.Parameters.Count;
            if (count != 0 || needsParameters)
            {
                w.Write("(");
            }

            foreach (CodeExpression expression in e.Parameters)
            {
				var arrayCreate = expression as CodeArrayCreateExpression;
				if (arrayCreate != null && arrayCreate.CreateType.BaseType == "System.Windows.Forms.DataGridViewColumn")
				{
					count = arrayCreate.Initializers.Count;
					foreach(CodeExpression init in arrayCreate.Initializers)
					{
						count--;
						GenerateCodeFromExpression(init, w, o);
						if (count > 0)
						{
							w.Write(",");
						}
					}

				}
				else
				{
					count--;
					GenerateCodeFromExpression(expression, w, o);
					if (count > 0)
					{
						w.Write(",");
					}
				}
            }

            if (e.Parameters.Count != 0 || needsParameters)
            {
                w.Write(")");
            }

            w.Write("\r\n");
        }

        private void GenerateCodeFromThisReferenceExpression(CodeThisReferenceExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            w.Write("$" + _currentType.Name);
        }

        private void GenerateCodeFromFieldReferenceExpression(CodeFieldReferenceExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.TargetObject is CodeThisReferenceExpression)
            {
                if (_currentType.Properties.All(m => m.Name != e.FieldName))
                {
                    w.Write("${0}.", _currentType.Name);
                }
                w.Write("$" + e.FieldName);
            }
            else if (e.TargetObject is CodeTypeReferenceExpression)
            {
                GenerateCodeFromExpression(e.TargetObject, w, o);
                w.Write("::" + e.FieldName);
            }
            else
            {
                GenerateCodeFromExpression(e.TargetObject, w, o);
                w.Write("." + e.FieldName);
            }
        }

        private void GenerateCodeFromPropertyReferenceExpression(CodePropertyReferenceExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.TargetObject is CodeThisReferenceExpression)
            {
                if (_currentType.Properties.All(m => m.Name != e.PropertyName))
                {
                    w.Write("${0}.", _currentType.Name);
                }
                w.Write(e.PropertyName);
            }
            else if (e.TargetObject is CodeTypeReferenceExpression)
            {
                GenerateCodeFromExpression(e.TargetObject, w, o);
                w.Write("::" + e.PropertyName);
            }
            else
            {
                GenerateCodeFromExpression(e.TargetObject, w, o);
                w.Write("." + e.PropertyName);
            }
        }

        private void GenerateCodeFromVariableReferenceExpression(CodeVariableReferenceExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            w.Write("$" + e.VariableName);
        }

        private void GenerateCodeFromPrimitiveExpression(CodePrimitiveExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.Value is string)
            {
                w.Write("[System.String]'{0}'", e.Value.ToString().Replace("\'", "\'\'"));
            }
            else if (e.Value is char)
            {
                w.Write("[System.Char]'{0}'", e.Value.ToString());
            }
            else if (e.Value is bool)
            {
                w.Write(((bool)e.Value) ? "$true" : "$false");
            }
			else if (e.Value is Single)
			{
				w.Write("[{1}]{0}", ((Single)e.Value).ToString(CultureInfo.InvariantCulture), e.Value.GetType().FullName);
			}
            else if (e.Value is Double)
            {
                w.Write("[{1}]{0}", ((Double)e.Value).ToString(CultureInfo.InvariantCulture), e.Value.GetType().FullName);
            }
            else if (e.Value == null)
            {
                w.Write("$null");
            }
            else
			{
				w.Write("[{1}]{0}", e.Value.ToString(), e.Value.GetType().FullName);
			}
        }

        private void GenerateCodeFromObjectCreateExpression(CodeObjectCreateExpression e, TextWriter w,
            CodeGeneratorOptions o)
        {

            if (e.CreateType.BaseType == "System.Decimal")
            {
                var test = (CodeArrayCreateExpression)e.Parameters[0];
                GenerateCodeFromExpression(test.Initializers[0], w, o);
                return;
            }

            w.Write("(New-Object -TypeName " + e.CreateType.BaseType);

            if (e.Parameters.Count > 0)
            {
                w.Write(" -ArgumentList @(");
                int count = e.Parameters.Count;
                foreach (CodeExpression parameter in e.Parameters)
                {
                    count--;
                    if (parameter is CodeArrayCreateExpression && count > 0)
                    {
                        w.Write(",");
                    }
                    GenerateCodeFromExpression(parameter, w, o);
                    if (count > 0)
                    {
                        w.Write(",");
                    }
                }
                w.Write(")");
            }

			w.Write(")");
        }

        public void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e is CodeAssignStatement)
            {
                GenereateCodeFromAssignStatement(e as CodeAssignStatement, w, o);
            }

            if (e is CodeCommentStatement)
            {
                GenerateCodeFromCommentState(e as CodeCommentStatement, w, o);
            }

            if (e is CodeExpressionStatement)
            {
                GenerateCodeFromExpression((e as CodeExpressionStatement).Expression, w, o);
            }

            if (e is CodeAttachEventStatement)
            {
                GenerateCodeFromCodeAttachEventStatement(e as CodeAttachEventStatement, w, o);
            }

			if (e is CodeVariableDeclarationStatement)
			{
				GenerateCodeFromCodeVariableDeclarationStatement(e as CodeVariableDeclarationStatement, w, o);
			}
        }

		private void GenerateCodeFromCodeVariableDeclarationStatement(CodeVariableDeclarationStatement e, TextWriter w, CodeGeneratorOptions o)
		{
			if (e.Name.Equals("resources"))
			{
				w.WriteLine($"$resources = . (Join-Path $PSScriptRoot '{ResourceShortFileName}')");
			}
			else
			{
				w.Write(string.Format("[{0}]${1} = ", e.Type.BaseType, e.Name));
				GenerateCodeFromExpression(e.InitExpression, w, o);
				w.Write(Environment.NewLine);
			}
		}

		private void GenerateCodeFromCodeCastExpression(CodeCastExpression e, TextWriter w, CodeGeneratorOptions o)
	    {
			w.Write("([" + e.TargetType.BaseType + "]");
			GenerateCodeFromExpression(e.Expression, w, o);
			w.Write(")");
	    }

        private void GenerateCodeFromCodeAttachEventStatement(CodeAttachEventStatement e, TextWriter w,
            CodeGeneratorOptions o)
        {
            if (e.Event.TargetObject is CodeThisReferenceExpression)
            {
                if (_currentType.Methods.All(m => m.Name != e.Event.EventName))
                {
                    w.Write("${0}.", _currentType.Name);
                }
            }
            else
            {
                GenerateCodeFromExpression(e.Event.TargetObject, w, o);
                w.Write(".");
            }

            w.Write("add_{0}(", e.Event.EventName);
            GenerateCodeFromExpression(e.Listener, w, o);
            w.Write(")" + Environment.NewLine);
        }


        private void GenerateCodeFromCommentState(CodeCommentStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            w.WriteLine("#" + e.Comment.Text);
        }

        private void GenereateCodeFromAssignStatement(CodeAssignStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.Left is CodePropertyReferenceExpression cpre && cpre.PropertyName.Equals("DoubleBuffered"))
            {
                return;
            }

            GenerateCodeFromExpression(e.Left, w, o);
            w.Write(" = ");
            GenerateCodeFromExpression(e.Right, w, o);
            w.Write("\r\n");
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
#if DEBUG
            var dcg = new DebugCodeGenerator();
            dcg.GenerateCodeFromNamespace(e, null, null);
#endif

            foreach (CodeTypeDeclaration type in e.Types)
            {
                GenerateCodeFromType(type, w, o);
            }
        }

        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            var fileName = powerShellCodeDomProvider.GetDesignerFileName(w);
            _ps1FileName = powerShellCodeDomProvider.GetCodeFileName(w);

            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                foreach (var reference in e.ReferencedAssemblies)
                {
                    writer.WriteLine("[void][System.Reflection.Assembly]::Load('{0}')", reference);
                }

                foreach (CodeNamespace name in e.Namespaces)
                {
                    GenerateCodeFromNamespace(name, writer, o);
                }

                writer.Flush();
            }

            GeneratePsd1FromResx();
        }

        public static class ResourceFileUpdater
        {
            private static FileSystemWatcher _watcher;
            private static string _targetFile;

            static ResourceFileUpdater()
            {
                _watcher = new FileSystemWatcher();
                _watcher.Changed += _watcher_Changed;
                _watcher.Deleted += _watcher_Changed;
                _watcher.Created += _watcher_Changed;
                _watcher.Renamed += _watcher_Changed;
            }

            public static void StartWatching(string fileName)
            {
                _targetFile = fileName;
                var directory = new FileInfo(fileName).DirectoryName;
                _watcher.Path = directory;
                _watcher.EnableRaisingEvents = true;
            }

            private static void _watcher_Changed(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath.Equals(_targetFile, StringComparison.OrdinalIgnoreCase) && e.ChangeType == WatcherChangeTypes.Renamed)
                {
                    for(var retry = 3; retry > 0; retry--)
                    {
                        try
                        {
                            var fileName = e.FullPath.Replace(".resx", ".resources.ps1");

                            var resxGenerator = new PowerShellResourceFileGenerator();
                            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                            {
                                var xdoc = XDocument.Load(e.FullPath);
                                resxGenerator.GenerateCode(xdoc, e.FullPath, writer);
                            }

                            _watcher.EnableRaisingEvents = false;
                        }
                        catch
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }
        }

        private void GeneratePsd1FromResx()
        {
            var resxFile = BaseFileName + ".resx";
            if (!File.Exists(resxFile)) return;

            ResourceFileUpdater.StartWatching(resxFile);
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            if (string.IsNullOrEmpty(_ps1FileName))
            {
                _ps1FileName = powerShellCodeDomProvider.GetCodeFileName(w);
            }

            _currentType = new TypeModel();
	        _currentType.Name = e.Name;
            if (e.BaseTypes.Count == 1)
            {
                w.WriteLine("${0} = New-Object -TypeName {1}", _currentType.Name, e.BaseTypes[0].BaseType);
                _currentType.Properties.Add(new MemberModel
                {
                    IsPublic = true,
                    Name = "base"
                });
            }

            CodeConstructor constructor = null;
            foreach (var member in e.Members)
            {
                if (member is CodeConstructor)
                {
                    constructor = member as CodeConstructor;
                }
                else if (member is CodeMemberMethod)
                {
                    GenerateCodeFromMethod(member as CodeMemberMethod, w, o);
                }
                else if (member is CodeMemberField)
                {
                    GenerateCodeFromField(member as CodeMemberField, w, o);
                }
            }

            if (constructor != null)
                GenerateCodeFromConstructor(constructor, w, o);
        }

        private void GenerateCodeFromConstructor(CodeConstructor e, TextWriter w, CodeGeneratorOptions o)
        {
            foreach (CodeStatement statement in e.Statements)
            {
                GenerateCodeFromStatement(statement, w, o);
            }
        }

        private void GenerateCodeFromMethod(CodeMemberMethod e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.UserData.Contains("DontGenerate")) return;

            //Must be event handler. Create in PS1 rather than Designer.ps1
            if (e.Name != "InitializeComponent")
            {
                AppendOrNoopToPs1(e.Name);
                return;
            }

            var method = new MemberModel();
            method.Name = e.Name;
            method.IsPublic = e.Attributes.HasFlag(MemberAttributes.Public);
            _currentType.Methods.Add(method);

            w.WriteLine("function " + e.Name);
            w.WriteLine("{");

            foreach (CodeStatement statement in e.Statements)
            {
                GenerateCodeFromStatement(statement, w, o);
            }

	        foreach (var property in _currentType.Properties)
	        {
                if (property.Name == "base") continue;
				w.WriteLine("Add-Member -InputObject ${0} -Name {1} -Value ${1} -MemberType NoteProperty", _currentType.Name, property.Name);
			}

            w.WriteLine("}");
        }

        private void AppendOrNoopToPs1(string functionName)
        {
            functionName = Regex.Replace(functionName, "[^a-zA-Z0-9_]", string.Empty);

            Token[] tokens;
            ParseError[] errors;
            var ast = Parser.ParseFile(_ps1FileName, out tokens, out errors);
            var functionExists = ast.FindAll(m => m is AssignmentStatementAst, false)
                .Cast<AssignmentStatementAst>()
                .Any(m => (m.Left as VariableExpressionAst)?.VariablePath.ToString().Equals(functionName, StringComparison.OrdinalIgnoreCase) == true);

            if (functionExists) return;

            powerShellCodeDomProvider.InsertIntoBeginningOfFile(_ps1FileName, $"${functionName} = {{\r\n\r\n}}\r\n\r\n");
        }

        private void GenerateCodeFromField(CodeMemberField e, TextWriter w, CodeGeneratorOptions o)
        {
            var member = new MemberModel();
            member.Name = e.Name;
            member.IsPublic = e.Attributes.HasFlag(MemberAttributes.Public);
            _currentType.Properties.Add(member);

            if (e.Type != null && e.Type.BaseType != "System.Void")
            {
                w.Write("[{0}]", e.Type.BaseType);
            }

            w.Write("$" + e.Name);

            if (e.InitExpression != null)
            {
                w.Write(" = ");
                GenerateCodeFromExpression(e.InitExpression, w, o);
                w.Write("\r\n");
            }
            else
            {
                w.Write(" = $null\r\n");
            }


        }

        private string BaseFileName
        {
            get
            {
                var fileInfo = new FileInfo(_ps1FileName);
                return fileInfo.FullName.Remove(fileInfo.FullName.Length - 4);
            }
        }

        private string ResourceShortFileName
        {
            get
            {
                var fileInfo = new FileInfo(_ps1FileName);
                return fileInfo.Name.Split('.').Take(fileInfo.Name.Split('.').Length - 1).Aggregate((x, y) => x + "." + y) + ".resources.ps1";
            }
        }
    }

    public class TypeModel
    {
        public TypeModel()
        {
            Methods = new List<MemberModel>();
            Properties = new List<MemberModel>();
        }

		public string Name { get; set; }
        public List<MemberModel> Methods { get; private set; }
        public List<MemberModel> Properties { get; private set; }
    }

    public class MemberModel
    {
        public bool IsPublic { get; set; }
        public string Name { get; set; }
    }
}
