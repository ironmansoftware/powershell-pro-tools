using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;

namespace PowerShellTools.Contracts
{
    [ComImport]
    [ComVisible(true)]
    [Guid("5D4ED25F-4C83-4BA5-A307-3953D43E3417")]
    public interface IPowerShellHostClientService
    {
        IPowerShellExplorerService ExplorerService { get; }
    }
}
