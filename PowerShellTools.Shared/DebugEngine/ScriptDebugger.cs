using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using Microsoft.VisualStudio.Shell;
using DTE = EnvDTE;
using DTE80 = EnvDTE80;
using PowerShellTools.Common.Debugging;
using System.Diagnostics;
using System.Windows.Forms;
using PowerShellTools.Common.Logging;
using System.Management.Automation;

namespace PowerShellTools.DebugEngine
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }

    public class EventArgs<T, T1> : EventArgs
    {
        public EventArgs(T value, T1 value2)
        {
            Value = value;
            Value2 = value2;
        }

        public T Value { get; private set; }
        public T1 Value2 { get; private set; }
    }

    /// <summary>
    /// This is the main debugger for PowerShell Tools for Visual Studio
    /// </summary>
    public partial class ScriptDebugger
    {
        private List<ScriptStackFrame> _callstack;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptDebugger));

        /// <summary>
        /// Event is fired when a debugger is paused.
        /// </summary>
        public event EventHandler<EventArgs<ScriptLocation>> DebuggerPaused;

        /// <summary>
        /// Event is fired when the debugger has finished.
        /// </summary>
        public event EventHandler<EventArgs> DebuggingFinished;

        /// <summary>
        /// Event is fired when the debugger has began.
        /// </summary>
        public event EventHandler DebuggingBegin;

        /// <summary>
        /// Event is fired when a terminating exception is thrown.
        /// </summary>
        public event EventHandler<EventArgs<PowerShellRunTerminatingException>> TerminatingException;

        /// <summary>
        /// The current set of variables for the current runspace.
        /// </summary>
        public IDictionary<string, Variable> Variables { get; private set; }

        /// <summary>
        /// The current call stack for the runspace.
        /// </summary>
        public IEnumerable<ScriptStackFrame> CallStack { get { return _callstack; } }

        /// <summary>
        /// The currently executing <see cref="ScriptProgramNode"/>
        /// </summary>
        public ScriptProgramNode CurrentExecutingNode { get; private set; }

        /// <summary>
        /// Indicate if debugger is ready for accepting command
        /// </summary>
        public bool IsDebuggingCommandReady { get; private set; }

        /// <summary>
        /// Indicate if there is on-going debugging, coz we should only allow one debugging session
        /// </summary>
        public bool IsDebugging { get; set; }

        /// <summary>
        /// Indicate if runspace is hosting remote session
        /// </summary>
        public bool RemoteSession { get; set; }

        public BreakpointManager BreakpointManager { get; set; }

        public string DebuggingCommand { get; set; }

        #region Debugging service event handlers

        /// <summary>
        /// Debugger stopped handler
        /// </summary>
        /// <param name="e"></param>
        public void DebuggerStop(DebuggerStoppedEventArgs e)
        {
            Log.InfoFormat("Debugger stopped");
            try
            {
                if (e.OpenScript)
                {
                    OpenFileInVS(e.ScriptFullPath);
                }

                RefreshScopedVariables();
                RefreshCallStack();

                if (e.BreakpointHit && !BreakpointManager.ProcessLineBreakpoints(e.ScriptFullPath, e.Line, e.Column))
                {
                    // Breakpoint not found. Continue...
                    DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_Continue);
                    IsDebuggingCommandReady = false;
                    return;
                }
                else if (!e.BreakpointHit)
                {
                    if (DebuggerPaused != null)
                    {
                        var scriptLocation = new ScriptLocation(e.ScriptFullPath, e.Line, 0);

                        DebuggerPaused(this, new EventArgs<ScriptLocation>(scriptLocation));
                    }
                }
            }
            catch (DebugEngineInternalException dbgEx)
            {
                Log.Debug(dbgEx.Message);
                DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_Stop);

                IsDebuggingCommandReady = false;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
                DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_Stop);

                IsDebuggingCommandReady = false;
                throw;
            }
            finally
            {
                Log.Debug("Waiting for debuggee to resume.");

                IsDebuggingCommandReady = true;
                System.Threading.Tasks.Task.Run(() =>
                {
                    RefreshPrompt();
                });
            }
        }

        /// <summary>
        /// PS execution terminating excpetion handler
        /// </summary>
        /// <param name="ex"></param>
        public void TerminateException(PowerShellRunTerminatingException ex)
        {
            if (TerminatingException != null)
            {
                // from editor debug run
                TerminatingException(this, new EventArgs<PowerShellRunTerminatingException>(ex));
            }
            else
            {
                // from REPL execution
                HostUi.VsOutputString(ex.Message);
            }
        }

        /// <summary>
        /// PSDebugger event finished handler
        /// </summary>
        public void DebuggerFinished()
        {
            IsDebuggingCommandReady = false;

            if (DebuggingFinished != null)
            {
                DebuggingFinished(this, new EventArgs());
            }
            NativeMethods.SetForegroundWindow();
        }

        public void DebuggerBegin()
        {
            if (DebuggingBegin != null)
            {
                DebuggingBegin(this, EventArgs.Empty);
            }
        }

        private void ConnectionExceptionHandler(object sender, EventArgs e)
        {
            Log.Error("Connection to host service is broken, terminating debugging.");
            DebuggerFinished();
        }

        #endregion

        /// <summary>
        /// Retrieve local scoped variable from debugger(in PSHost proc)
        /// </summary>
        public void RefreshScopedVariables()
        {
            try
            {
                IEnumerable<Variable> vars = DebuggingService.GetScopedVariable();
                Variables = new Dictionary<string, Variable>();
                foreach (Variable v in vars)
                {
                    Variables.Add(v.VarName, v);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to refresh scoped variables.", ex);
            }
        }

        /// <summary>
        /// Retrieve callstack info from debugger(in PSHost proc)
        /// </summary>
        private void RefreshCallStack()
        {
            IEnumerable<CallStack> result = null;
            try
            {
                if (IsDebugging)
                {
                    result = DebuggingService.GetCallStack();
                }
                else
                {
                    throw new DebugEngineInternalException();
                }

                _callstack = new List<ScriptStackFrame>();
                if (result == null) return;

                foreach (var psobj in result)
                {
                    _callstack.Add(
                        new ScriptStackFrame(
                            CurrentExecutingNode,
                            psobj.ScriptFullPath,
                            psobj.FrameString,
                            psobj.StartLine,
                            psobj.EndLine,
                            psobj.StartColumn,
                            psobj.EndColumn));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to refresh callstack", ex);
                throw;
            }
        }

        /// <summary>
        /// Stops execution of the current script.
        /// </summary>
        public void Stop()
        {
            Log.Info("Stop");

            try
            {
                if (IsDebuggingCommandReady)
                {
                    DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_Stop);
                }
                else
                {
                    DebuggingService.Stop();
                }

                IsDebuggingCommandReady = false;
                Log.Info("Stop complete.");
            }
            catch (Exception ex)
            {
                //BUGBUG: Suppressing an exception that is thrown when stopping...
                Log.Debug("Error while stopping script...", ex);
            }
            finally
            {
                DebuggerFinished();
            }
        }

        /// <summary>
        /// Stop over block.
        /// </summary>
        public void StepOver()
        {
            Log.Info("StepOver");
            DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_StepOver);
            IsDebuggingCommandReady = false;
        }

        /// <summary>
        /// Step into block.
        /// </summary>
        public void StepInto()
        {
            Log.Info("StepInto");
            DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_StepInto);
            IsDebuggingCommandReady = false;
        }

        /// <summary>
        /// Step out of block.
        /// </summary>
        public void StepOut()
        {
            Log.Info("StepOut");
            DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_StepOut);
            IsDebuggingCommandReady = false;
        }

        /// <summary>
        /// Continue execution.
        /// </summary>
        public void Continue()
        {
            Log.Info("Continue");
            DebuggingService.SetDebuggerResumeAction(DebugEngineConstants.Debugger_Continue);
            IsDebuggingCommandReady = false;
        }

        public Collection<T> Execute<T>(string commandLine)
        {
            Log.Info($"Execute<{typeof(T)}>");

            try
            {
                IsDebuggingCommandReady = false;
                return DebuggingService.Execute<T>(commandLine);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to execute script", ex);
                HostUi.VsOutputString(ex.Message);
                return default;
            }
            finally
            {
                DebuggerFinished();
            }
        }

        public IEnumerable<Variable> GetVariableDetails(string path, int skip, int take)
        {
            return DebuggingService.GetVariableDetails(path, skip, take);
        }

        public int GetVariableDetailsCount(string path)
        {
            return DebuggingService.GetVariableDetailsCount(path);
        }


        /// <summary>
        /// Execute the specified command line.
        /// </summary>
        /// <param name="commandLine">Command line to execute.</param>
        public bool Execute(string commandLine)
        {
            Log.Info("Execute");

            try
            {
                return ExecuteInternal(commandLine);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to execute script", ex);
                HostUi.VsOutputString(ex.Message);
                return false;
            }
            finally
            {
                DebuggerFinished();
            }
        }

        /// <summary>
        /// Execute the specified command line
        /// </summary>
        /// <param name="commandLine">Command line to execute.</param>
        public bool ExecuteInternal(string commandLine)
        {
            IsDebuggingCommandReady = false;
            return DebuggingService.Execute(commandLine);
        }

        /// <summary>
        /// Execute the specified command line as debugging command.
        /// </summary>
        /// <param name="commandLine">Command line to execute.</param>
        public void ExecuteDebuggingCommand(string commandLine)
        {
            Log.Info("Execute debugging command");

            if (IsDebuggingCommandReady)
            {
                try
                {
                    DebuggingService.ExecuteDebuggingCommandOutDefault(commandLine);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to execute debugging command", ex);
                }
            }
        }

        /// <summary>
        /// Execute the current program node.
        /// </summary>
        /// <remarks>
        /// The node will either be a script file or script content; depending on the node
        /// passed to this function.
        /// </remarks>
        /// <param name="node"></param>
        public void Execute(ScriptProgramNode node)
        {
            CurrentExecutingNode = node;

            if (node.IsAttachedProgram)
            {
                string result = string.Empty;
                if (!node.IsRemoteProgram)
                {
                    result = DebuggingService.AttachToRunspace(node.Process.ProcessId);
                }
                else
                {
                    result = DebuggingService.AttachToRemoteRunspace(node.Process.ProcessId, node.Process.HostName);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    // if either of the attaches returns an error, let the user know
                    MessageBox.Show(result, Resources.AttachErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // see what state we are in post error, if not in a local state, we need to try and get there
                    DebugScenario postCleanupScenario = DebuggingService.GetDebugScenario();

                    // try as hard as we can to detach/cleanup the mess for the length of CleanupRetryTimeout
                    TimeSpan retryTimeSpan = TimeSpan.FromMilliseconds(DebugEngineConstants.CleanupRetryTimeout);
                    Stopwatch timeElapsed = Stopwatch.StartNew();
                    while (timeElapsed.Elapsed < retryTimeSpan && postCleanupScenario != DebugScenario.Local)
                    {
                        postCleanupScenario = DebuggingService.CleanupAttach();
                    }

                    // if our efforts to cleanup the mess were unsuccessful, inform the user
                    if (postCleanupScenario != DebugScenario.Local)
                    {
                        MessageBox.Show(Resources.CleanupErrorMessage, Resources.DetachErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    RefreshPrompt();
                    DebuggerFinished();
                }
            }
            else
            {
                string commandLine = node.FileName;

                if (node.IsFile)
                {
                    commandLine = String.Format(DebugEngineConstants.ExecutionCommandFormat, node.FileName, node.Arguments);
                    HostUi.VsOutputString(string.Format("{0}{1}{2}", GetPrompt(), node.FileName, Environment.NewLine));
                }

                Execute(commandLine);
            }
        }

        public void SetVariable(string name, string value)
        {
            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = _runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", name).AddParameter("Value", value);
                    powerShell.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set variable.", ex);
            }
        }


        public Variable GetVariable(string name)
        {
            if (name.StartsWith("$"))
            {
                name = name.Remove(0, 1);
            }

            if (Variables.ContainsKey(name))
            {
                var var = Variables[name];
                return var;
            }

            return null;
        }

        internal void OpenFileInVS(string fullName)
        {
            var dte2 = (DTE80.DTE2)Package.GetGlobalService(typeof(DTE.DTE));

            if (dte2 != null)
            {
                try
                {
                    if (!dte2.ItemOperations.IsFileOpen(fullName))
                    {
                        dte2.ItemOperations.OpenFile(fullName);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(DebugScenarioUtilities.ScenarioToFileOpenErrorMsg(DebuggingService.GetDebugScenario()), ex);
                    HostUi.VsOutputString(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Location within a script.
    /// </summary>
    public class ScriptLocation
    {
        /// <summary>
        /// The full path to the file.
        /// </summary>
        public string File { get; set; }
        /// <summary>
        /// Line number within the file.
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// Column within the file.
        /// </summary>
        public int Column { get; set; }

        public ScriptLocation(string file, int line, int column)
        {
            File = file;
            Line = line;
            Column = column;
        }
    }
}