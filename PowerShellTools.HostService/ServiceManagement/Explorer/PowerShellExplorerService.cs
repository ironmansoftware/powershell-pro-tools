using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.HostService.ServiceManagement
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [PowerShellServiceHostBehavior]
    public sealed class PowerShellExplorerService : IPowerShellExplorerService
    {
        private IPowerShellExplorerServiceCallback _callback;
        private static object _syncLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        public PowerShellExplorerService() { }

        /// <summary>
        /// Ctor (unit test hook)
        /// </summary>
        /// <param name="callback">Callback context object (unit test hook)</param>
        public PowerShellExplorerService(IPowerShellExplorerServiceCallback callback)
            : this()
        {
            _callback = callback;
        }

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
