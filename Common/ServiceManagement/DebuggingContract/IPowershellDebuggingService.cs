using PowerShellProTools.Host;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    public interface IPowerShellDebuggingService
    {
        void SetBreakpoint(PowerShellBreakpoint bp);


        void EnableBreakpoint(PowerShellBreakpoint bp, bool enable);


        void RemoveBreakpoint(PowerShellBreakpoint bp);


        void RemoveBreakpointById(int id);


        void ClearBreakpoints();


        bool Execute(string cmdline);

        Collection<T> Execute<T>(string commandLine);


        string ExecuteDebuggingCommandOutDefault(string cmdline);

        string ExecuteDebuggingCommandOutNull(string cmdline);

        void SetDebuggerResumeAction(DebuggerResumeAction resumeAction);

        void Stop();


        void SetRunspace(bool overrideExecutionPolicy);

        bool IsAttachable(uint pid);


        string AttachToRunspace(uint pid);


        bool DetachFromRunspace();


        List<KeyValuePair<uint, string>> EnumerateRemoteProcesses(string remoteMachine, ref string errorMessage, bool useSSL);


        string AttachToRemoteRunspace(uint pid, string remoteName);


        bool DetachFromRemoteRunspace();


        DebugScenario CleanupAttach();


        IEnumerable<Variable> GetScopedVariable();

        IEnumerable<CallStack> GetCallStack();

        string GetPrompt();

        RunspaceAvailability GetRunspaceAvailability();

        int GetPSBreakpointId(PowerShellBreakpoint bp);


        void SetOption(PowerShellRawHostOptions option);


        DebugScenario GetDebugScenario();


        string GetTrueFileName(string file);

        void LoadProfiles();

        IEnumerable<Variable> GetVariableDetails(string path, int skip, int take);
        int GetVariableDetailsCount(string path);
        IEnumerable<Module> GetModules();
        IEnumerable<Module> GetImportedModules();
        void ImportModule(Module module);
        event EventHandler ExecuteFinished;
        event EventHandler DebuggerStopped;
    }

}
