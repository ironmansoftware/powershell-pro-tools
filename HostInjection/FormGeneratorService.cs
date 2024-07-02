using IMS.FormDesigner;
using PowerShellProTools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace PowerShellProTools.Host
{
    public class FormGeneratorService : IFormGeneratorService
    {
        public void GenerateWinForm(string codeFilePath, string formPath, bool package)
        {
            var formGenerator = new FormGenerator();

            var text = File.ReadAllText(codeFilePath);

            var scriptBlock = ScriptBlock.Create(text);

            var path = formPath;
            var designer = formPath.Replace(".ps1", ".designer.ps1");

            var designerOutput = formGenerator.GenerateForm(scriptBlock);
            var logicOutput = formGenerator.GenerateLogic(text, codeFilePath, designer);

            File.WriteAllText(path, logicOutput);
            File.WriteAllText(designer, designerOutput);
        }
    }
}
