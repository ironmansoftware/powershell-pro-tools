using System;
using System.Runtime.InteropServices;

namespace PowerShellTools.Contracts
{
    [ComImport]
    [ComVisible(true)]
    [Guid("cf108f4f-2e2a-44ad-907d-9a01905f7d8e")]
    public interface IPowerShellService
    {
        bool ExecutePowerShellCommand(string command);
        bool ExecutePowerShellCommand(string command, Action<string> output);
    }
}
