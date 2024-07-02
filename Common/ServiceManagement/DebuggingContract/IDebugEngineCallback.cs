using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    public interface IDebugEngineCallback
    {
        void OutputString(string output);
        void OutputStringLine(string output);
        void BreakpointUpdated(DebuggerBreakpointUpdatedEventArgs args);
        void OutputProgress(long sourceId, ProgressRecord record);
        void DebuggerStopped(DebuggerStoppedEventArgs args);
        void TerminatingException(PowerShellRunTerminatingException ex);

        void DebuggerFinished();

        void RefreshPrompt();

        Task<string> ReadHostPromptAsync(string message, string name, bool secure = false);

        int ReadHostPromptForChoices(string caption, string message, IList<ChoiceItem> choices, int defaultChoice);

        Task<PSCredential> ReadSecureStringPromptAsync(
            string message,
            string name);

        PSCredential GetPSCredentialPrompt(
            string caption,
            string message,
            string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options);

        Task RequestUserInputOnStdIn();

        Task OpenRemoteFileAsync(string fullName);

        void SetRemoteRunspace(bool enabled);

        void ClearHostScreen();

        VsKeyInfo VsReadKey();

        bool IsKeyAvailable();

        int GetREPLWindowWidth();
    }
}
