using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Event handlers for debugger events inside service
    /// </summary>
    public partial class PowerShellDebuggingService
    {
        /// <summary>
        /// Runspace state change event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            ServiceCommon.Log("Runspace State Changed: {0}", e.RunspaceStateInfo.State);

            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    if (_callback != null)
                    {
                        _callback.DebuggerFinished();
                    }
                    break;
            }
        }

        /// <summary>
        /// Runspace availability change handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _runspace_AvailabilityChanged(Object sender, RunspaceAvailabilityEventArgs e)
        {
            ServiceCommon.Log("Runspace Availability Changed: {0}", e.RunspaceAvailability.ToString());
        }

        /// <summary>
        /// Breakpoint updates (such as enabled/disabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            // Not in-use for now, leave it as place holder for future support on powershell debugging command in REPL
            ServiceCommon.Log("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            //if (_callback != null)
            //{
            //    var lbp = e.Breakpoint as LineBreakpoint;
            //    _callback.BreakpointUpdated(new DebuggerBreakpointUpdatedEventArgs(new PowershellBreakpoint(e.Breakpoint.Script, lbp.Line, lbp.Column), e.UpdateType));
            //}
        }

        /// <summary>
        /// Debugging output event handler
        /// </summary>
        /// <param name="value">String to output</param>
        public void NotifyOutputString(string value)
        {
            ServiceCommon.LogCallbackEvent("Callback to client for string output in VS");
            if (_callback != null)
            {
                _callback.OutputString(value);
            }
        }

        /// <summary>
        /// Debugging output event handler, to show progress status.
        /// </summary>
        /// <param name="sourceId">The id of the record with progress.</param>
        /// <param name="record">The record itself.</param>
        public void NotifyOutputProgress(long sourceId, ProgressRecord record)
        {
            ServiceCommon.LogCallbackEvent("Callback to client to show progress");

            if (_callback != null)
            {
                _callback.OutputProgress(sourceId, record);
            }
        }

        /// <summary>
        /// PS debugger stopped event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            ServiceCommon.Log("Debugger stopped ...");
            DebugScenario currScenario = GetDebugScenario();

            if (_installedPowerShellVersion < RequiredPowerShellVersionForRemoteSessionDebugging)
            {
                RefreshScopedVariable();
                RefreshCallStack();
            }
            else
            {
                RefreshScopedVariable40();
                RefreshCallStack40();
            }

            ServiceCommon.LogCallbackEvent("Callback to client, and wait for debuggee to resume");
            if (e.Breakpoints.Count > 0)
            {
                LineBreakpoint bp = (LineBreakpoint)e.Breakpoints[0];
                if (_callback != null)
                {
                    string file = bp.Script;
                    if (currScenario != DebugScenario.Local && _mapRemoteToLocal.ContainsKey(bp.Script))
                    {
                        file = _mapRemoteToLocal[bp.Script];
                    }

                    // breakpoint is always hit for this case
                    _callback.DebuggerStopped(new DebuggerStoppedEventArgs(file, bp.Line, bp.Column, true, false));
                }
            }
            else
            {                
                if (_callback != null)
                {
                    string file;
                    int lineNum, column;

                    switch (currScenario)
                    {
                        case DebugScenario.LocalAttach:
                            file = e.InvocationInfo.ScriptName;
                            lineNum = e.InvocationInfo.ScriptLineNumber;
                            column = e.InvocationInfo.OffsetInLine;

                            // the stop which occurs after attaching is not associated with a breakpoint and should result in the process' script being opened
                            _callback.DebuggerStopped(new DebuggerStoppedEventArgs(file, lineNum, column, false, true));
                            break;
                        case DebugScenario.RemoteAttach:
                            // copy the remote file over to host machine
                            file = OpenRemoteAttachedFile(e.InvocationInfo.ScriptName);
                            lineNum = e.InvocationInfo.ScriptLineNumber;
                            column = e.InvocationInfo.OffsetInLine;

                            // the stop which occurs after attaching is not associated with a breakpoint and should result in the remote process' script being opened
                            _callback.DebuggerStopped(new DebuggerStoppedEventArgs(file, lineNum, column, false, true));
                            _needToCopyRemoteScript = false;
                            break;
                        default:
                            _callback.DebuggerStopped(new DebuggerStoppedEventArgs());
                            break;
                    }
                }
            }

            bool resumed = false;
            while (!resumed)
            {
                _pausedEvent.WaitOne();

                try
                {
                    currScenario = GetDebugScenario();
                    if (!string.IsNullOrEmpty(_debuggingCommand))
                    {
                        if (currScenario == DebugScenario.Local)
                        {
                            // local debugging
                            var output = new Collection<PSObject>();

                            using (var pipeline = (_runspace.CreateNestedPipeline()))
                            {
                                pipeline.Commands.AddScript(_debuggingCommand);
                                pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                                output = pipeline.Invoke();
                            }

                            ProcessDebuggingCommandResults(output);
                        }
                        else
                        {
                            // remote session and local attach debugging
                            ProcessRemoteDebuggingCommandResults(ExecuteDebuggingCommand());
                        }
                    }
                    else
                    {
                        ServiceCommon.Log(string.Format("Debuggee resume action is {0}", _resumeAction));
                        e.ResumeAction = _resumeAction;
                        resumed = true; // debugger resumed executing
                    }
                }
                catch (Exception ex)
                {
                    NotifyOutputString(ex.Message);
                }

                // Notify the debugging command execution call that debugging command was complete.
                _debugCommandEvent.Set();
            }
        }

        private PSDataCollection<PSObject> ExecuteDebuggingCommand()
        {
            PSCommand psCommand = new PSCommand();
            psCommand.AddScript(_debuggingCommand);
            psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            var output = new PSDataCollection<PSObject>();
            output.DataAdded += objects_DataAdded;
            _runspace.Debugger.ProcessCommand(psCommand, output);

            return output;
        }

        private void ProcessDebuggingCommandResults(Collection<PSObject> output)
        {
            if (output != null && output.Count > 0)
            {
                StringBuilder outputString = new StringBuilder();
                foreach (PSObject obj in output)
                {
                    if (obj != null)
                        outputString.AppendLine(obj.ToString());
                }

                if (_debugOutput)
                {
                    NotifyOutputString(outputString.ToString());
                }

                var pobj = output.FirstOrDefault();

                if (pobj != null && pobj.BaseObject is string)
                {
                    _debugCommandOutput = (string)pobj.BaseObject;
                }
                else if (pobj != null && pobj.BaseObject is LineBreakpoint)
                {
                    LineBreakpoint bp = (LineBreakpoint)pobj.BaseObject;
                    if (bp != null)
                    {
                        _psBreakpointTable.Add(
                            new PowerShellBreakpointRecord(
                                new PowerShellBreakpoint(bp.Script, bp.Line, bp.Column),
                                bp.Id));
                    }
                }
            }
        }

        private void ProcessRemoteDebuggingCommandResults(PSDataCollection<PSObject> output)
        {
            var pobj = output.FirstOrDefault();

            if (pobj != null && pobj.BaseObject is string)
            {
                _debugCommandOutput = (string)pobj.BaseObject;
            }
            else if (pobj != null && pobj.BaseObject is LineBreakpoint)
            {
                LineBreakpoint bp = (LineBreakpoint)pobj.BaseObject;
                if (bp != null)
                {
                    _psBreakpointTable.Add(
                        new PowerShellBreakpointRecord(
                            new PowerShellBreakpoint(bp.Script, bp.Line, bp.Column),
                            bp.Id));
                }
            }
        }

        /// <summary>
        /// Handling the remote file open event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="args">remote file name</param>
        private void HandleRemoteSessionForwardedEvent(object sender, PSEventArgs args)
        {
            if (args.SourceIdentifier.Equals("PSISERemoteSessionOpenFile", StringComparison.OrdinalIgnoreCase))
            {
                string text = null;
                byte[] array = null;
                try
                {
                    if (args.SourceArgs.Length == 2)
                    {
                        text = (args.SourceArgs[0] as string);
                        array = (byte[])(args.SourceArgs[1] as PSObject).BaseObject;
                    }
                    if (!string.IsNullOrEmpty(text) && array != null)
                    {
                        string tmpFileName = Path.GetTempFileName();
                        string dirPath = tmpFileName.Remove(tmpFileName.LastIndexOf('.'));
                        Directory.CreateDirectory(dirPath);
                        string fullFileName = Path.Combine(dirPath, new FileInfo(text).Name);

                        _mapRemoteToLocal[text] = fullFileName;
                        _mapLocalToRemote[fullFileName] = text;

                        File.WriteAllBytes(fullFileName, array);

                        _callback.OpenRemoteFile(fullFileName);
                    }
                }
                catch (Exception ex)
                {
                    ServiceCommon.Log("Failed to create local copy for downloaded file due to exception: {0}", ex.Message);
                }
            }
        }
    }
}
