using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.ExplorerContract
{
    public interface IPowerShellExplorerService
    {
        Task<List<IPowerShellModule>> GetModules();
        Task<List<IPowerShellCommand>> GetCommands();
        Task<string> GetCommandHelp(IPowerShellCommand command);
        Task<IPowerShellCommandMetadata> GetCommandMetadata(IPowerShellCommand command);
    }
}
