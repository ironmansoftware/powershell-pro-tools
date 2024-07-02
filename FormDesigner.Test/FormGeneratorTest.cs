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
    }
}
