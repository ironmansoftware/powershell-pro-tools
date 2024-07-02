using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using PowerShellTools.Repl;
using Common;
using Thread = System.Threading.Thread;

namespace PowerShellTools.DebugEngine
{
#if POWERSHELL
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using PowerShellProTools.Host;
    using PowerShellTools.Common.Debugging;
    using PowerShellTools.Common.ServiceManagement.DebuggingContract;
    using PowerShellTools.CredentialUI;
    using PowerShellTools.DebugEngine.PromptUI;
    using PowerShellTools.ServiceManagement;
    using IReplWindow = IPowerShellReplWindow;
#endif

    /// <summary>
    /// The PoshTools PowerShell host and debugger; the part that interaces with the host (Visual Studio).
    /// </summary>
    public partial class ScriptDebugger
    {
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly CultureInfo _originalCultureInfo = Thread.CurrentThread.CurrentCulture;
        private readonly CultureInfo _originalUiCultureInfo = Thread.CurrentThread.CurrentUICulture;
        private Runspace _runspace;
        private IPowerShellDebuggingService _debuggingServiceTest;

        public IPowerShellDebuggingService DebuggingService
        {
            get
            {
                if (_debuggingServiceTest != null)
                {
                    return _debuggingServiceTest;
                }

                return PowerShellToolsPackage.DebuggingService;
            }
            private set
            {
                _debuggingServiceTest = value;
            }
        }

        public Runspace Runspace
        {
            get
            {
                return _runspace;
            }
            set
            {
                _runspace = value;
            }
        }

        public ScriptDebugger(bool overrideExecutionPolicy, ConnectionManager connectionManager)
            : this(overrideExecutionPolicy, null, connectionManager)
        {
            if (connectionManager != null)
            {
                connectionManager.ConnectionException += ConnectionExceptionHandler;
            }
        }

        internal ScriptDebugger(bool overrideExecutionPolicy, IPowerShellDebuggingService debuggingServiceTestHook, ConnectionManager connectionManager)
        {
            OverrideExecutionPolicy = overrideExecutionPolicy;
            _debuggingServiceTest = debuggingServiceTestHook;

            try
            {
                DebuggingService.SetRunspace(overrideExecutionPolicy);
            }
            catch
            {
                Log.Warn("Overriding the execution policy failed. Attempting again without policy set.");
                DebuggingService.SetRunspace(false);
                OverrideExecutionPolicy = false;
            }


            //TODO: remove once user prompt work is finished for debugging
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            HostUi = new HostUi();

            BreakpointManager = new BreakpointManager();

            NativeMethods.SetForegroundWindow();
        }

        public HostUi HostUi { get; private set; }

        public bool OverrideExecutionPolicy { get; private set; }

        public IReplWindow ReplWindow
        {
            get { return HostUi.ReplWindow; }
            set
            {
                HostUi.ReplWindow = value;
                if (value != null)
                {
                    RefreshPrompt();
                }
            }
        }

        /// <summary>
        ///     Refreshes the prompt in the REPL window to match the current PowerShell prompt value.
        /// </summary>
        public void RefreshPrompt()
        {
            var prompt = GetPrompt();

            //CONSOLE: if (HostUi != null)
            //{
            //    if (!licensed)
            //    {
            //        Console.WriteLine("This PowerShell Console is a feature of PowerShell Pro Tools. You can install a license to remove this message.");
            //    }

            //    if (Console.CursorLeft > 0)
            //    {
            //        Console.WriteLine();
            //    }

            //    Console.Write(prompt);
            //    Console.SetCursorPosition(prompt.Length, Console.CursorTop);
            //}

            if (HostUi != null && HostUi.ReplWindow != null && prompt != null)
            {
                HostUi.ReplWindow.SetOptionValue(ReplOptions.CurrentPrimaryPrompt, prompt);
            }
        }

        private string GetPrompt()
        {
            try
            {
                string prompt = string.Empty;

                if (DebuggingService != null)
                {
                    if (IsDebuggingCommandReady)
                    {
                        prompt = DebuggingService.ExecuteDebuggingCommandOutNull(DebugEngineConstants.GetPrompt);
                        if (prompt == null && DebuggingService.GetRunspaceAvailability() == RunspaceAvailability.Available)
                        {
                            prompt = DebuggingService.GetPrompt();
                        }
                    }
                    else if (DebuggingService.GetRunspaceAvailability() == RunspaceAvailability.Available)
                    {
                        prompt = DebuggingService.GetPrompt();
                    }

                    return prompt;
                }

                return prompt;
            }
            catch
            {
                return string.Empty;
            }
        }
    }


    /// <summary>
    /// Hanldes interaction with the HostUi.
    /// </summary>
    public class HostUi
    {
        public IReplWindow ReplWindow { get; set; }

        private static readonly object AnimationIconGeneralIndex = (short)STATUSBARCONSTS.SBAI_Gen; //General Status Bar Animation
        private static readonly object AnimationProgressSyncObject = new object();  // Needed to keep the animation count correct.

        private static HashSet<long> _animationProgressSources = new HashSet<long>();

        internal void VSOutputProgress(long sourceId, ProgressRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            var statusBar = (IVsStatusbar)PowerShellToolsPackage.Instance?.GetService(typeof(SVsStatusbar));

            if (statusBar != null)
            {
                uint cookie = 0;

                ProgressRecordType progressStatus = record.RecordType;

                string label = string.Format(ResourceStrings.ProgressBarFormat, record.Activity, record.StatusDescription);

                switch (progressStatus)
                {
                    case ProgressRecordType.Processing:
                        {
                            if (record.PercentComplete >= 0 && record.PercentComplete < 100)
                            {
                                statusBar.Progress(ref cookie, 1, label, (uint)record.PercentComplete, 100);
                            }
                            else if (record.PercentComplete == 100)
                            {
                                statusBar.Progress(ref cookie, 1, "", 0, 0);
                            }
                            else
                            {
                                // According to PS ProgressRecord docs, Negative values means a progress bar should not be displayed.

                                lock (AnimationProgressSyncObject)
                                {
                                    if (_animationProgressSources.Add(sourceId)) //Returns false if already exists.
                                    {
                                        // This is needed because Visual Studio keeps a count of each animation.
                                        // Animation is removed only when count goes to zero.
                                        statusBar.Animation(1, AnimationIconGeneralIndex);
                                    }

                                    statusBar.SetText(label);
                                }
                            }
                            //Currently, we do not show Seconds Remaining
                            break;
                        }
                    case ProgressRecordType.Completed:
                        {
                            //Only other value is ProgressRecordType.Completed

                            if (record.PercentComplete >= 0)
                            {
                                statusBar.Progress(ref cookie, 0, string.Empty, 0, 0);
                            }
                            else
                            {
                                lock (AnimationProgressSyncObject)
                                {
                                    if (_animationProgressSources.Remove(sourceId))  //returns false if item not found.
                                    {
                                        statusBar.Animation(0, AnimationIconGeneralIndex);
                                    }

                                    statusBar.SetText(label);
                                }
                            }
                            break;
                        }
                }
            }
        }

        private IVsOutputWindowPane _generalPane;

        /// <summary>
        /// output string into output window (general pane)
        /// </summary>
        /// <param name="output">string to output</param>
        private void OutputString(string output)
        {
            try
            {
                if (_generalPane == null)
                {
                    IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                    Guid generalPaneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
                    // By default this is no pane created in output window, so we need to create one by our own
                    // This call won't do anything if there is one exists
                    int hr = outWindow.CreatePane(generalPaneGuid, "General", 1, 1);
                    outWindow.GetPane(ref generalPaneGuid, out _generalPane);
                }

                if (_generalPane != null)
                {
                    _generalPane.Activate(); // Brings this pane into view
                    _generalPane.OutputStringThreadSafe(output); // Thread-safe so the the output order can be preserved
                }
            }
            catch { }

        }

        /// <summary>
        /// Read host from user input
        /// </summary>
        /// <param name="message">Prompt dialog message</param>
        /// <param name="name">Parameter Name if any</param>
        /// <returns>User input string</returns>
        public async Task<string> ReadLineAsync(string message, string name, bool secure)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string input = string.Empty;

            ReadHostPromptDialogViewModel viewModel = new ReadHostPromptDialogViewModel(message, name, secure);
            ReadHostPromptDialog dialog = new ReadHostPromptDialog(viewModel);

            var ret = dialog.ShowModal();

            if (ret.HasValue && ret.Value == true)
            {
                input = viewModel.ParameterValue;
            }

            return input;
        }

        public int ReadChoice(string caption, string message, IList<ChoiceItem> choices, int defaultChoice)
        {
            if (string.IsNullOrEmpty(caption))
            {
                caption = ResourceStrings.PromptForChoice_DefaultCaption;
            }

            if (message == null)
            {
                message = string.Empty;
            }

            if (choices == null)
            {
                throw new ArgumentNullException("choices");
            }

            if (!choices.Any())
            {
                throw new ArgumentException(string.Format(ResourceStrings.ChoicesCollectionShouldHaveAtLeastOneElement, "choices"), "choices");
            }

            foreach (var c in choices)
            {
                if (c == null)
                {
                    throw new ArgumentNullException("choices");
                }
            }

            if (defaultChoice < -1 || defaultChoice >= choices.Count)
            {
                throw new ArgumentOutOfRangeException("defaultChoice");
            }

            int choice = -1;

            ReadHostPromptForChoicesViewModel viewModel = new ReadHostPromptForChoicesViewModel(caption, message, choices, defaultChoice);
            ReadHostPromptForChoicesView dialog = new ReadHostPromptForChoicesView(viewModel);

            NativeMethods.SetForegroundWindow();
            var ret = dialog.ShowModal();
            if (ret == true)
            {
                choice = viewModel.UserChoice;
            }
            return choice;
        }

        /// <summary>
        /// Ask for securestring from user
        /// </summary>
        /// <param name="message">Message of dialog window.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <returns>A PSCredential object that contains the securestring.</returns>
        public async Task<PSCredential> ReadSecureStringAsPSCredentialAsync(string message, string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SecureString secString = new SecureString();
            SecureStringDialogViewModel viewModel = new SecureStringDialogViewModel(message, name);
            SecureStringDialog dialog = new SecureStringDialog(viewModel);

            var ret = dialog.ShowModal();
            if (ret.HasValue && ret.Value == true)
            {
                secString = viewModel.SecString;
            }

            return new PSCredential("securestring", secString);
        }

        /// <summary>
        /// Ask for PSCredential from user
        /// </summary>
        /// <param name="caption">The caption for the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for. If this parameter set to null or an empty string, the function will prompt for the user name first.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <param name="allowedCredentialTypes">A bitwise combination of the PSCredentialTypes enumeration values that identify the types of credentials that can be returned.</param>
        /// <param name="options">A bitwise combination of the PSCredentialUIOptions enumeration values that identify the UI behavior when it gathers the credentials.</param>
        /// <returns>A PSCredential object that contains the credentials for the target.</returns>
        public PSCredential GetPSCredential(string caption, string message, string userName,
            string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            PSCredential result = null;

            CredentialsDialog dialog = new CredentialsDialog(targetName, caption, message);
            dialog.Name = userName;

            switch (options)
            {
                case PSCredentialUIOptions.AlwaysPrompt:
                    dialog.AlwaysDisplay = true;
                    break;
                case PSCredentialUIOptions.ReadOnlyUserName:
                    dialog.KeepName = true;
                    break;
                case PSCredentialUIOptions.Default:
                case PSCredentialUIOptions.None:
                    break;
                default:
                    break;
            }

            if (dialog.Show() == DialogResult.OK)
            {
                result = new PSCredential(dialog.Name, dialog.Password);
            }

            return result;
        }

        /// <summary>
        /// Output string from debugger in VS output/REPL pane window
        /// </summary>
        /// <param name="output"></param>
        public void VsOutputString(string output)
        {
            if (ReplWindow != null)
            {
                if (output.StartsWith(PowerShellConstants.PowerShellOutputErrorTag))
                {
                    ReplWindow.WriteError(output);
                }
                else
                {
                    ReplWindow.WriteOutput(output);
                }
            }

            var ansiStart = output.IndexOf("\x1b");
            var ansiEnd = ansiStart == -1 ? 0 : output.IndexOf("m", ansiStart);

            while (ansiStart != -1)
            {
                output = output.Remove(ansiStart, ansiEnd - ansiStart + 1);
                ansiStart = output.IndexOf("\x1b");
                ansiEnd = ansiStart == -1 ? 0 : output.IndexOf("m", ansiStart);
            }

            OutputString(output);
        }
    }
}