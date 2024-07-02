using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using PowerShellTools.Contracts;

namespace PowerShellTools.Service
{
    internal sealed class PowerShellHostClientService : IPowerShellHostClientService
    {
        public PowerShellHostClientService()
        {
        }

        public IPowerShellExplorerService ExplorerService
        {
            get { return PowerShellToolsPackage.ExplorerService; }
        }
    }
}
