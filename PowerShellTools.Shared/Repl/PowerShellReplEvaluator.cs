using Microsoft.VisualStudio.Text;
using PowerShellTools.Classification;
using PowerShellTools.DebugEngine;
using System.Threading.Tasks;

namespace PowerShellTools.Repl
{
#if POWERSHELL
    using PowerShellTools.Common.Debugging;
    using PowerShellTools.Options;
    using IReplEvaluator = IPowerShellReplEvaluator;
    using IReplWindow = IPowerShellReplWindow;
    using ReplRoleAttribute = PowerShellReplRoleAttribute;
#endif

    [ReplRole("Debug")]
    internal class PowerShellReplEvaluator : IReplEvaluator
    {
        public IReplWindow Window { get; set; }

        public ScriptDebugger Debugger
        {
            get
            {
                return PowerShellToolsPackage.Debugger;
            }
        }

        private TaskFactory<ExecutionResult> tf = new TaskFactory<ExecutionResult>();

        public void Dispose()
        {
        }

        public async Task<ExecutionResult> Initialize(IReplWindow window)
        {
            await Task.Run(
                    () =>
                    {
                        PowerShellToolsPackage.DebuggerReadyEvent.WaitOne();
                        Debugger.ReplWindow = window;
                    }
                );

            var page = await GeneralOptions.GetLiveInstanceAsync();

            window.TextView.Properties.AddProperty(BufferProperties.FromRepl, null);

            window.SetOptionValue(ReplOptions.Multiline, page.MultilineRepl);
            window.SetOptionValue(ReplOptions.UseSmartUpDown, true);
            window.SetOptionValue(ReplOptions.SupportAnsiColors, true);

            return await tf.StartNew(() => { 
                Window = window;
                return new ExecutionResult(true); 
            });

        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {

        }

        public Task<ExecutionResult> Reset()
        {
            return tf.StartNew(() => new ExecutionResult(true));
        }

        public bool CanExecuteText(string text)
        {
            return true;
        }

        public async Task<ExecutionResult> ExecuteText(string text)
        {
            await Task.CompletedTask;
            if (text.Trim().Equals("cls", System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals("Clear-Host", System.StringComparison.OrdinalIgnoreCase))
            {
                Window.ClearScreen();
                return ExecutionResult.Success;
            }

            if (Debugger.IsDebuggingCommandReady)
            {
                _ = Task.Run(() =>
                {
                    Debugger.ExecuteDebuggingCommand(text);
                });

                return new ExecutionResult(true);
            }
            else
            {
                _ = Task.Run(() =>
                {
                    Debugger.Execute(text);
                });
                
                return new ExecutionResult(true);
            }
        }

        public void ExecuteFile(string filename)
        {

        }

        public string FormatClipboard()
        {
            return null;
        }

        public Task<ExecutionResult> AbortCommand()
        {
            return tf.StartNew(() =>
            {
                Debugger.Stop();
                return new ExecutionResult(true);
            });
        }

        public Task<ExecutionResult> EnterRemoteSession(string computerName)
        {
            string cmdEnterRemoteSession = string.Format(DebugEngineConstants.EnterRemoteSessionDefaultCommand, computerName);
            return ExecuteText(cmdEnterRemoteSession);
        }

        public Task<ExecutionResult> ExitRemoteSession()
        {
            string cmdExitRemoteSession = string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand);
            return ExecuteText(cmdExitRemoteSession);
        }

        public bool IsRemoteSession()
        {
            return Debugger.RemoteSession;
        }

        public bool IsDebuggerInitialized()
        {
            return Debugger != null;
        }

        public void OpenConsoleWindow()
        {
            PowerShellToolsPackage.Instance.ShowConsoleWindow();
        }
    }

}
