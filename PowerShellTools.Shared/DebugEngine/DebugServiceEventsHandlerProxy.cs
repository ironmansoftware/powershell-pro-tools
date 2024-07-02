using System.Collections.Generic;
using System.Management.Automation;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using TTask = System.Threading.Tasks.Task;
using PowerShellTools.Common.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows;
using System.Threading.Tasks;
using System.Security;

namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// Proxy of debugger service event handlers
    /// This works as InstanceContext for debugger service channel
    /// </summary>
    public class DebugServiceEventsHandlerProxy : IDebugEngineCallback
    {
        private ScriptDebugger _debugger;
        private bool _uiOutput;
        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugServiceEventsHandlerProxy));
        private Thread outputThread;
        private BlockingCollection<string> outputCollection = new BlockingCollection<string>();
        private CancellationTokenSource outputThreadSource = new CancellationTokenSource();

        public DebugServiceEventsHandlerProxy(ScriptDebugger debugger, bool output)
        {
            _debugger = debugger;
            _uiOutput = output;
        }

        public DebugServiceEventsHandlerProxy()
        {
            _uiOutput = true;

            Application.Current.Exit += (sender, args) =>
            {
                outputCollection.CompleteAdding();
                outputThreadSource.Cancel();
            };

            outputThread = new Thread(() =>
            {
                try
                {
                    while (!outputCollection.IsAddingCompleted)
                    {
                        var outputStr = outputCollection.Take(outputThreadSource.Token);
                        try
                        {
                            Debugger.HostUi.VsOutputString(outputStr);
                        }
                        catch { }
                    }
                }
                catch { }
            });
            outputThread.Start();
        }

        public ScriptDebugger Debugger
        {
            get
            {
                if (_debugger == null)
                {
                    _debugger = PowerShellToolsPackage.Debugger;
                }
                return _debugger;
            }
        }


        /// <summary>
        /// To open specific file in client
        /// </summary>
        /// <param name="fullName">Full name of remote file(mapped into local)</param>
        public async TTask OpenRemoteFileAsync(string fullName)
        {
            await TTask.CompletedTask;
            Debugger.OpenFileInVS(fullName);
        }

        public void SetRemoteRunspace(bool enabled)
        {
            Log.DebugFormat("{0}({1})", nameof(SetRemoteRunspace), enabled);
            Debugger.RemoteSession = enabled;
        }

        public void ClearHostScreen()
        {
            Log.DebugFormat("{0}()", nameof(ClearHostScreen));
            if (Debugger.ReplWindow != null)
            {
                Debugger.ReplWindow.ClearScreen();
            }
        }

        public async Task<PSCredential> ReadSecureStringPromptAsync(string message, string name)
        {
            var value = await ReadHostPromptAsync(message, name);
            var secureString = new SecureString();
            foreach(var c in value)
            {
                secureString.AppendChar(c);
            }

            return new PSCredential("secure", secureString);
        }

        public PSCredential GetPSCredentialPrompt(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return Debugger.HostUi.GetPSCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
        }

        public VsKeyInfo VsReadKey()
        {
            if (Debugger.ReplWindow != null)
            {
                return Debugger.ReplWindow.WaitKey();
            }
            else
            {
                return null;
            }
        }

        public bool IsKeyAvailable()
        {
            bool isAvailable = false;
            if (Debugger?.ReplWindow != null)
            {
                isAvailable = Debugger.ReplWindow.IsKeyAvailable();
            }

            return isAvailable;
        }

        public int GetREPLWindowWidth()
        {
            int width = PowerShellTools.Common.Constants.MinimalReplBufferWidth;
            if (Debugger?.ReplWindow != null)
            {
                width = Debugger.ReplWindow.GetRawHostBufferWidth();
            }

            return width;
        }

        public async Task<string> ReadHostPromptAsync(string message, string name, bool secure = false)
        {
            return await Debugger.HostUi.ReadLineAsync(message, name, secure);
        }

        public int ReadHostPromptForChoices(string caption, string message, IList<ChoiceItem> choices, int defaultChoice)
        {
            return Debugger.HostUi.ReadChoice(caption, message, choices, defaultChoice);
        }

        public async Task RequestUserInputOnStdIn()
        {
            string inputText = await Debugger.HostUi.ReadLineAsync(Resources.UserInputRequestMessage, string.Empty, false);

            // Feed into stdin stream
            if (PowerShellToolsPackage.ConnectionManager != null && PowerShellToolsPackage.ConnectionManager.HostProcess != null)
            {
                PowerShellToolsPackage.ConnectionManager.HostProcess.WriteHostProcessStandardInputStream(inputText);
            }
        }

        public void DebuggerStopped(DebuggerStoppedEventArgs args)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("{0}({1})", nameof(DebuggerStopped), JsonConvert.SerializeObject(args));
            }
            
            Debugger.DebuggerStop(args);
        }

        public void BreakpointUpdated(DebuggerBreakpointUpdatedEventArgs args)
        {
            Debugger.BreakpointManager.UpdateBreakpoint(args);
        }

        public void OutputString(string output)
        {
            outputCollection.Add(output);
        }

        public void OutputStringLine(string output)
        {
            outputCollection.Add(output);
        }

        public void OutputProgress(long sourceId, ProgressRecord record)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                Debugger.HostUi.VSOutputProgress(sourceId, record);
            });
        }

        public void TerminatingException(PowerShellRunTerminatingException ex)
        {
            Debugger.HostUi.VsOutputString(ex.Message);
        }

        public void DebuggerFinished()
        {
            Debugger.DebuggerFinished();
        }

        public void RefreshPrompt()
        {
            Debugger.RefreshPrompt();
        }
    }
}
