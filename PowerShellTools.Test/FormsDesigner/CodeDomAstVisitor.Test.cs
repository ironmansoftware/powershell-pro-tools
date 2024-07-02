using PowerShellToolsPro.FormsDesigner;
using System.CodeDom;
using System.IO;
using System.Management.Automation.Language;
using Xunit;

namespace PowerShellToolsPro.Test.FormsDesigner
{
    public class CodeDomAstVisitorTest
    {
        [Fact]
        public void ShouldParseSapienExport()
        {
            var codeDomVisitor = new SapienCodeDomAstVisitor();

            string result;
            using (Stream stream = GetType().Assembly.GetManifestResourceStream("PowerShellTools.Test.Assets.form.Export.ps1"))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            Token[] tokens;
            ParseError[] errors;
            var ast = Parser.ParseInput(result, out tokens, out errors);

            var codeDom = codeDomVisitor.Parse(ast) as CodeCompileUnit;
        }

        [Fact]
        public void ShouldCreateCorrectResourceNode()
        {
            var codeDomVisitor = new CodeDomAstVisitor();

            Token[] tokens;
            ParseError[] errors;
            var ast = Parser.ParseInput(@"
$MainForm = New-Object -TypeName System.Windows.Forms.Form
function InitializeComponent
{
$Icon = ([System.Drawing.Icon]$resources.'$this.Icon')
}", out tokens, out errors);

            var codeDom = codeDomVisitor.Visit(ast) as CodeCompileUnit;

            var initialize = codeDom.Namespaces[0].Types[0].Members[1] as CodeMemberMethod;
            var codeAssignStatement = initialize.Statements[0] as CodeAssignStatement;
            var variableReference = codeAssignStatement.Left as CodeVariableReferenceExpression;

            Assert.Equal("Icon", variableReference.VariableName);

            var castExpression = codeAssignStatement.Right as CodeCastExpression;
            var methodInvoke = castExpression.Expression as CodeMethodInvokeExpression;

            Assert.Equal("resources", (methodInvoke.Method.TargetObject as CodeVariableReferenceExpression).VariableName);
            Assert.Equal("GetObject", methodInvoke.Method.MethodName);
            Assert.Equal("$this.Icon", (methodInvoke.Parameters[0] as CodePrimitiveExpression).Value);
        }

        [Fact]
        public void ShouldCreateCorrectResourceVariable()
        {
            var codeDomVisitor = new CodeDomAstVisitor();

            Token[] tokens;
            ParseError[] errors;
            var ast = Parser.ParseInput(@"
$MainForm = New-Object -TypeName System.Windows.Forms.Form
function InitializeComponent
{
$resources = Invoke-Expression (Get-Content ""$PSScriptRoot\MultiThreadedForm.Designer.psd1"" -Raw)
}", out tokens, out errors);

            var codeDom = codeDomVisitor.Visit(ast) as CodeCompileUnit;

            var initialize = codeDom.Namespaces[0].Types[0].Members[1] as CodeMemberMethod;
            var codeVariableDeclaration = initialize.Statements[0] as CodeVariableDeclarationStatement;
           
            Assert.Equal("resources", codeVariableDeclaration.Name);

            var objectCreate = codeVariableDeclaration.InitExpression as CodeObjectCreateExpression;
            
            Assert.Equal("System.ComponentModel.ComponentResourceManager", objectCreate.CreateType.BaseType);
            Assert.Equal("MainForm", (objectCreate.Parameters[0] as CodeTypeOfExpression).Type.BaseType);
        }
    }
}
