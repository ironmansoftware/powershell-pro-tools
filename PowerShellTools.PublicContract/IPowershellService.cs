using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Contracts
{
    [ComImport]
    [ComVisible(true)]
    [Guid("cf108f4f-2e2a-44ad-907d-9a01905f7d8e")]
    public interface IPowerShellService
    {
        bool ExecutePowerShellCommand(string command);

        Task<bool> ExecutePowerShellCommandAsync(string command);

        bool ExecutePowerShellCommand(string command, Action<string> output);

        Task<bool> ExecutePowerShellCommandAsync(string command, Action<string> output);
    }
}
