using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace PowerShellTools.MSBuild
{
    public class RunPowerShellCommand : Task
    {
        [Required]
        public ITaskItem Command { get; set; }

        public override bool Execute()
        {
            var host = new MSBuildPowerShellHost(s =>
            {
                if (!string.IsNullOrEmpty(s))
                    this.Log.LogMessage(MessageImportance.High, s);
            });

            try
            {
                var runspace = RunspaceFactory.CreateRunspace(host);
                runspace.Open();

                using (var powershell = PowerShell.Create())
                {
                    powershell.AddScript(Command.ItemSpec);

                    powershell.AddCommand("out-default");
                    powershell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                    powershell.Invoke();
                }

            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }
    }
}
