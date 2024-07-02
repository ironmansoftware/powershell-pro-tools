using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using PowerShellTools.Common;
using PowerShellTools.Contracts;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.Explorer
{
    internal sealed class DataProvider : IDataProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DataProvider));
        private IPowerShellHostClientService _powerShellHostClientService;

        public DataProvider()
        {
        }

        private IPowerShellHostClientService Host
        {
            get
            {
                if (_powerShellHostClientService == null)
                {
                    _powerShellHostClientService = Package.GetGlobalService(typeof(IPowerShellHostClientService)) as IPowerShellHostClientService;
                }

                return _powerShellHostClientService;
            }
        }

        public async void GetModules(Action<List<IPowerShellModule>> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetModules();

                callback(data);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrive modules", ex);
            }
        }

        public async void GetCommands(Action<List<IPowerShellCommand>> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommands();

                callback(data);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrive commands", ex);
            }
        }

        public async void GetCommandHelp(IPowerShellCommand command, Action<string> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommandHelp(command);

                callback(data);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrive command help", ex);
            }
        }

        public async void GetCommandMetaData(IPowerShellCommand command, Action<IPowerShellCommandMetadata> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommandMetadata(command);

                callback(data);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrive command metadata", ex);
            }
        }
    }
}
