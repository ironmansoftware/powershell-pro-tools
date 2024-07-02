using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// PowerShell v4.0+ only specific utilities
    /// </summary>
    public partial class PowerShellDebuggingService
    {
        private void RefreshScopedVariable40()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all local variable in scope");
            if (_runspace.ConnectionInfo != null)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript("Get-Variable");
                var output = new PSDataCollection<PSObject>();
                DebuggerCommandResults results = _runspace.Debugger.ProcessCommand(psCommand, output);
                _varaiables = output;
            }
            else
            {
                RefreshScopedVariable();
            }
        }

        private void RefreshCallStack40()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all call stack frames");
            if (_runspace.ConnectionInfo != null)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript("Get-PSCallstack");
                var output = new PSDataCollection<PSObject>();
                DebuggerCommandResults results = _runspace.Debugger.ProcessCommand(psCommand, output);
                _callstack = output;
            }
            else
            {
                RefreshCallStack();
            }
        }

        private void SetRemoteScriptDebugMode40(System.Management.Automation.Runspaces.Runspace runspace)
        {
            runspace.Debugger.SetDebugMode(DebugModes.RemoteScript);
        }
    }
}
