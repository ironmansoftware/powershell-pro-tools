using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;

namespace PowerShellTools.HostService.ServiceManagement
{
    public sealed class PowerShellExplorerService : IPowerShellExplorerService
    {
        /// <summary>
        public async Task<List<IPowerShellModule>> GetModules()
        {
            var myTask = Task.Factory.StartNew(() =>
                ExplorerExecutionHelper.ExecuteCommand<PSModuleInfo>("Get-Module -ListAvailable")
                );

            var result = await myTask;
            return ConversionFactory.Convert(result);
        }

        public async Task<List<IPowerShellCommand>> GetCommands()
        {
            var myTask = Task.Factory.StartNew(() => 
                ExplorerExecutionHelper.ExecuteCommand<CommandInfo>("Get-Command")
                );

            var result = await myTask;
            return ConversionFactory.Convert(result);
        }

        public async Task<string> GetCommandHelp(IPowerShellCommand command)
        {
            var script = string.Format("Get-Help -Name {0} -Full | Out-String", command.Name);

            var myTask = Task.Factory.StartNew(() =>
                ExplorerExecutionHelper.ExecuteCommand<string>(script)
                );

            var result = await myTask;

            if (result.Count > 0)
            {
                return result[0];
            }

            return string.Empty;
        }

        public async Task<IPowerShellCommandMetadata> GetCommandMetadata(IPowerShellCommand command)
        {
            var script = string.Format("New-object System.Management.Automation.CommandMetaData -ArgumentList (Get-Command {0})", command.Name);

            var myTask = Task.Factory.StartNew(() =>
                ExplorerExecutionHelper.ExecuteCommand<CommandMetadata>(script)
                );

            var result = await myTask;
            var items = ConversionFactory.Convert(result);

            if (items.Count > 0)
            {
                return items[0];
            }

            return new PowerShellCommandMetadata();
        }
    }
}
