using IMS.FormDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PowerShellToolsPro.Test.FormsDesigner
{
    public class FormGeneratorTest
    {
        [Fact]
        public void ShouldGenerateLogicWithSingleParameterSet()
        {
            var scriptBlock = ScriptBlock.Create("function Test { param($Field, [Switch]$Test, [System.DayOfWeek]$Test2) }");

            var formGenerator = new FormGenerator();
            var formCode = formGenerator.GenerateLogic(scriptBlock);

            var expected = "";

            //Assert.Equal(expected, formCode);
        }

        [Fact]
        public void ShouldGenerateFormWithSingleParameterSet()
        {
            var scriptBlock = ScriptBlock.Create("function Test { param($Field, [Switch]$Test, [System.DayOfWeek]$Test2) }");

            var formGenerator = new FormGenerator();
            var formCode = formGenerator.GenerateForm(scriptBlock);

            var expected = "";

            //Assert.Equal(expected, formCode);
        }

        [Fact]
        public void ShouldInsertStubAfterRequiresUsingAndParamBlock()
        {
            var script = "#requires -version 5.1\r\nusing namespace System.Windows.Forms\r\nparam(\r\n    [string]$Name\r\n)\r\nWrite-Host $Name";
            var stub = "$button1_Click = {\r\n\r\n}\r\n\r\n";

            var result = PowerShellCodeDomProvider.InsertTextAfterScriptPreamble(script, stub);

            Assert.Equal("#requires -version 5.1\r\nusing namespace System.Windows.Forms\r\nparam(\r\n    [string]$Name\r\n)\r\n$button1_Click = {\r\n\r\n}\r\n\r\nWrite-Host $Name", result);
        }

        [Fact]
        public void ShouldInsertStubAfterSingleLineParamBlock()
        {
            var script = "param([string]$Name)\r\nWrite-Host $Name";
            var stub = "function button1_Click {\r\n}\r\n\r\n";

            var result = PowerShellCodeDomProvider.InsertTextAfterScriptPreamble(script, stub);

            Assert.Equal("param([string]$Name)\r\nfunction button1_Click {\r\n}\r\n\r\nWrite-Host $Name", result);
        }

        [Fact]
        public void ShouldInsertStubAtBeginningWhenNoPreambleExists()
        {
            var script = "Write-Host 'Ready'";
            var stub = "$button1_Click = {\r\n\r\n}\r\n\r\n";

            var result = PowerShellCodeDomProvider.InsertTextAfterScriptPreamble(script, stub);

            Assert.Equal("$button1_Click = {\r\n\r\n}\r\n\r\nWrite-Host 'Ready'", result);
        }
    }
}
