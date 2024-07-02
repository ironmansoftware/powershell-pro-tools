using IMS.FormDesigner;
using System.IO;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsData.ConvertTo, "WinForm")]
    public class ConvertToWinFormCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "filePath")]
        public string Path { get; set; }

        [Parameter(ParameterSetName = "def")]
        public ScriptBlock FunctionDefinition { get; set; }

        [Parameter(ValueFromPipeline = true, ParameterSetName = "functionInfo", Mandatory = true)]
        public FunctionInfo Function { get; set; }

        [Parameter(ValueFromPipeline = true, ParameterSetName = "cmdletInfo", Mandatory = true)]
        public CmdletInfo Cmdlet { get; set; }

        [Parameter]
        public string OutputPath { get; set; }

        [Parameter]
        public SwitchParameter PackageAsExecutable { get; set; }


        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                OutputPath = "form.ps1";
            }

            var path = GetUnresolvedProviderPathFromPSPath(OutputPath);
            var designer = path.Replace(".ps1", ".designer.ps1");

            var formGenerator = new FormGenerator();

            string designerOutput = null, logicOutput = null;

            if (ParameterSetName == "filePath")
            {
                var inputPath = GetUnresolvedProviderPathFromPSPath(Path);

                var text = File.ReadAllText(inputPath);

                var scriptBlock = ScriptBlock.Create(text);

                designerOutput = formGenerator.GenerateForm(scriptBlock);
                logicOutput = formGenerator.GenerateLogic(text, inputPath, designerOutput);
            }

            if (ParameterSetName == "functionInfo")
            {
                designerOutput = formGenerator.GenerateForm(Function);
                logicOutput = formGenerator.GenerateLogic(Function);
            }

            if (ParameterSetName == "def")
            {
                designerOutput = formGenerator.GenerateForm(FunctionDefinition);
                logicOutput = formGenerator.GenerateLogic(FunctionDefinition);
            }

            if (ParameterSetName == "cmdletInfo")
            {
                designerOutput = formGenerator.GenerateForm(Cmdlet);
                logicOutput = formGenerator.GenerateLogic(Cmdlet);
            }

            File.WriteAllText(path, logicOutput);
            File.WriteAllText(designer, designerOutput);

            if (PackageAsExecutable)
            {
                var packageConfig = new Packager.Config.PsPackConfig();
                packageConfig.Root = path;
                packageConfig.OutputPath = new FileInfo(path).DirectoryName;
                packageConfig.Bundle.Enabled = true;
                packageConfig.Bundle.Modules = true;
                packageConfig.Package.Enabled = true;
                var process = new Packager.PackageProcess();
                process.Config = packageConfig;
                process.OnErrorMessage += Process_OnErrorMessage;
                process.OnMessage += Process_OnMessage;
                process.OnWarningMessage += Process_OnWarningMessage;
                process.Execute();
            }
        }

        private void Process_OnWarningMessage(object sender, string e)
        {
            WriteWarning(e);
        }

        private void Process_OnMessage(object sender, string e)
        {
            WriteInformation(new InformationRecord(null, e));
        }

        private void Process_OnErrorMessage(object sender, string e)
        {
            WriteError(new ErrorRecord(new System.Exception(e), string.Empty, ErrorCategory.SyntaxError, null));
        }
    }
}
