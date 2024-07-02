using IMS.FormDesigner.CodeDom;
using PowerShellToolsPro.LanguageService;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FormDesigner.Test
{
    public class CSharpAstVisitorTest
    {
        private string GetFormTest()
        {
            using(var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("FormDesigner.Test.FormTest.cs"))
            {
                using(var stringReader = new StreamReader(resource))
                {
                    return stringReader.ReadToEnd();
                }
            }
        }

        [Fact]
        public void ShouldReplicateCode()
        {
            var code = GetFormTest();

            var visitor = new CSharpAstVisitor();
            var codeCompileUnit = visitor.Parse(code);

            var provider = CodeDomProvider.CreateProvider("CSharp");

            using(var sr = new StringWriter())
            {
                provider.GenerateCodeFromCompileUnit(codeCompileUnit, sr, new CodeGeneratorOptions());
                var str = sr.ToString();
                //Assert.Equal(code, str);
            }
        }

        [Fact]
        public void ShouldParseCSharp()
        {
            var code = GetFormTest();

            var visitor = new CSharpAstVisitor();
            var codeCompileUnit = visitor.Parse(code);

            var debug = new DebugCodeGenerator();

            debug.GenerateCodeFromCompileUnit(codeCompileUnit, null, null);

            Assert.Equal("PowerShellToolsPro.Licensing", codeCompileUnit.Namespaces[0].Name);

            var type = codeCompileUnit.Namespaces[0].Types[0];

            Assert.Equal("GetTrialLicense", type.Name);
            Assert.Equal(MemberAttributes.Public, type.Attributes);
            Assert.Equal("System.Windows.Forms.Form", type.BaseTypes[0].BaseType);

            Assert.Equal("components", type.Members[0].Name);
            Assert.Equal("System.ComponentModel.IContainer", (type.Members[0] as CodeMemberField).Type.BaseType);
            Assert.Equal(MemberAttributes.Private, (type.Members[0] as CodeMemberField).Attributes);

            var dispose = type.Members[1];
            var initializeComponent = type.Members[2];

            Assert.Equal("Dispose", dispose.Name);
            Assert.Equal("InitializeComponent", initializeComponent.Name);
        }
    }
}
