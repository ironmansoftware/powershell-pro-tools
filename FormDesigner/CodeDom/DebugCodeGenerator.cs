using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PowerShellToolsPro.LanguageService
{
    public class DebugCodeGenerator : ICodeGenerator
    {
        public bool IsValidIdentifier(string value)
        {
            return true;
        }

        public void ValidateIdentifier(string value)
        {

        }

        public string CreateEscapedIdentifier(string value)
        {
            return value;
        }

        public string CreateValidIdentifier(string value)
        {
            return value;
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            return type.BaseType;
        }

        public bool Supports(GeneratorSupport supports)
        {
            return true;
        }

        public void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e == null) return;

            using (var logger = new StackDebugTracer())
            {
                if (e is CodePrimitiveExpression)
                {
                    PrintCodePrimitiveExpression(e as CodePrimitiveExpression);
                }

                else if (e is CodePropertyReferenceExpression)
                {
                    PrintCodePropertyReferenceExpression(e as CodePropertyReferenceExpression);
                }

                else if (e is CodeObjectCreateExpression)
                {
                    PrintCodeObjectCreateExpression(e as CodeObjectCreateExpression);
                }

                else if (e is CodeFieldReferenceExpression)
                {
                    PrintCodeFieldReferenceExpression(e as CodeFieldReferenceExpression);
                }

                else if (e is CodeThisReferenceExpression)
                {
                    logger.WriteLine("this");
                }

                else if (e is CodeMethodInvokeExpression)
                {
                    PrintCodeMethodInvokeExpression(e as CodeMethodInvokeExpression);
                }

                else
                {
                    logger.WriteLine(e.GetType().Name);
                }
            }
        }

        private void PrintCodeMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            using (var logger = new StackDebugTracer())
            {
                GenerateCodeFromExpression(e.Method.TargetObject, null, null);
                logger.WriteLine("Method: " + e.Method.MethodName);
                foreach (CodeExpression parameters in e.Parameters)
                {
                    GenerateCodeFromExpression(parameters, null, null);
                }
            }
        }

        private void PrintCodeFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("FieldName: " + e.FieldName);
                logger.WriteLine("TargetObject: ");
                GenerateCodeFromExpression(e.TargetObject, null, null);
            }
        }

        private void PrintCodeObjectCreateExpression(CodeObjectCreateExpression e)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("Type: " + e.CreateType.BaseType);
                logger.WriteLine("Parameters: ");
                foreach (CodeExpression parameter in e.Parameters)
                {
                    GenerateCodeFromExpression(parameter, null, null);
                }
            }
        }

        private void PrintCodePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("PropertyName: " + e.PropertyName);
                logger.WriteLine("TargetObject: ");
                GenerateCodeFromExpression(e.TargetObject, null, null);
            }
        }

        private void PrintCodePrimitiveExpression(CodePrimitiveExpression e)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine(e.Value?.ToString());
            }
        }

        public void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e == null) return;

            using (var logger = new StackDebugTracer())
            {
                

                if (e is CodeAssignStatement)
                {
                    PrintCodeAssignStatement(e as CodeAssignStatement);
                }

                else if (e is CodeCommentStatement)
                {
                    PrintCodeCommentStatement(e as CodeCommentStatement);
                }

                else if (e is CodeExpressionStatement)
                {
                    PrintCodeAssignStatement(e as CodeExpressionStatement);
                }

                else
                {
                    logger.WriteLine(e.GetType().Name);
                }
            }
        }

        private void PrintCodeAssignStatement(CodeExpressionStatement e)
        {
            GenerateCodeFromExpression(e.Expression, null, null);
        }


        private void PrintCodeAssignStatement(CodeAssignStatement e)
        {
            GenerateCodeFromExpression(e.Left, null, null);
            GenerateCodeFromExpression(e.Right, null, null);
        }

        private void PrintCodeCommentStatement(CodeCommentStatement e)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine(e.Comment.Text);
            }
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeNamespace:" + e.Name);

                foreach (CodeCommentStatement comment in e.Comments)
                {
                    PrintComment(comment);
                }

                foreach (CodeTypeDeclaration type in e.Types)
                {
                    GenerateCodeFromType(type, w, o);
                }
            }
        }

        private void PrintComment(CodeCommentStatement statement)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeCommentStatement: " + statement.Comment.Text);
            }
        }

        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeCompileUnit");

                foreach (CodeAttributeDeclaration attribute in e.AssemblyCustomAttributes)
                {
                    PrintCodeAttributeDeclartion(attribute);
                }

                logger.WriteLine("Referenced Assemblies: ");
                foreach (String assembly in e.ReferencedAssemblies)
                {
                    logger.WriteLine("\t" + assembly);
                }

                foreach (CodeNamespace ns in e.Namespaces)
                {
                    GenerateCodeFromNamespace(ns, w, o);
                }
            }
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeTypeDeclaration: " + e.Name);
                foreach (CodeTypeReference typeReference in e.BaseTypes)
                {
                    PrintCodeTypeReference(typeReference);
                }

                logger.WriteLine("Custom Attributes:");
                foreach (CodeAttributeDeclaration attrib in e.CustomAttributes)
                {
                    PrintCodeAttributeDeclartion(attrib);
                }

                foreach (CodeTypeMember member in e.Members)
                {
                    PrintCodeTypeMember(member);
                }
            }
        }

        private void PrintCodeTypeMember(CodeTypeMember member)
        {
            if (member is CodeMemberField)
            {
                PrintCodeMemberField(member as CodeMemberField);
            }
            else if (member is CodeConstructor)
            {
                PrintCodeMemberConstructor(member as CodeConstructor);
            }
            else if (member is CodeMemberMethod)
            {
                PrintCodeMemberMethod(member as CodeMemberMethod);
            }
            else if (member is CodeMemberProperty)
            {
                PrintCodeMemberProperty(member as CodeMemberProperty);
            }
        }

        private void PrintCodeMemberProperty(CodeMemberProperty property)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeMemberProperty: " + property.Name);
            }
        }

        private void PrintCodeMemberConstructor(CodeConstructor method)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeMemberConstructor: " + method.Name);

                foreach (CodeStatement statement in method.Statements)
                {
                    GenerateCodeFromStatement(statement, null, null);
                }
            }
        }

        private void PrintCodeMemberMethod(CodeMemberMethod method)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeMemberMethod: " + method.Name);

                foreach (CodeStatement statement in method.Statements)
                {
                    GenerateCodeFromStatement(statement, null, null);
                }
            }
        }

        private void PrintCodeMemberField(CodeMemberField field)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine(String.Format("CodeMemberField:[{0}] {1}", field.Type.BaseType, field.Name));
                GenerateCodeFromExpression(field.InitExpression, null, null);
            }
        }

        private void PrintCodeTypeReference(CodeTypeReference reference)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeTypeReference: " + reference.BaseType);
            }
        }

        private void PrintCodeAttributeDeclartion(CodeAttributeDeclaration attribute)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeAttributeDeclaration");
                foreach (CodeAttributeArgument arg in attribute.Arguments)
                {
                    PrintCodeAttributeArgument(arg);
                }
            }
        }

        private void PrintCodeAttributeArgument(CodeAttributeArgument argument)
        {
            using (var logger = new StackDebugTracer())
            {
                logger.WriteLine("CodeAttributeArgument: " + argument.Name);
                GenerateCodeFromExpression(argument.Value, null, null);
            }
        }
    }

    public class StackDebugTracer : IDisposable
    {
        private static Stack<StackDebugTracer> _stack = new Stack<StackDebugTracer>();

        public StackDebugTracer()
        {
            _stack.Push(this);
        }

        public void WriteLine(string message)
        {
            string tabs = String.Empty;
            for (int i = 0; i < _stack.Count; i++)
            {
                tabs += "\t";
            }

            Console.WriteLine(tabs + message);
        }

        public void Dispose()
        {
            _stack.Pop();
        }
    }
}
