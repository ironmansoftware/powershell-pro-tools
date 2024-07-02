using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// Breakpoint manager to handle variaties of operations on breakpoint in powershell runspace
    /// </summary>
    public class BreakpointManager
    {
        private List<ScriptBreakpoint> _breakpoints;
        private ScriptDebugger _debugger;
        private static readonly ILog Log = LogManager.GetLogger(typeof(BreakpointManager));

        /// <summary>
        /// Event is fired when a breakpoint is hit.
        /// </summary>
        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;

        /// <summary>
        /// Event is fired when a breakpoint is updated.
        /// </summary>
        public event EventHandler<DebuggerBreakpointUpdatedEventArgs> BreakpointUpdated;

        public ScriptDebugger Debugger
        {
            get
            {
                if (_debugger == null)
                    return PowerShellToolsPackage.Debugger;

                return _debugger;
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public BreakpointManager()
        {
            _breakpoints = new List<ScriptBreakpoint>();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="debugger">Script debugger</param>
        public BreakpointManager(ScriptDebugger debugger)
            : this()
        {
            _debugger = debugger;
        }

        /// <summary>
        /// Sets breakpoints for the current runspace.
        /// </summary>
        /// <remarks>
        /// This method clears any existing breakpoints.
        /// </remarks>
        /// <param name="initialBreakpoints"></param>
        public void SetBreakpoints(IEnumerable<ScriptBreakpoint> initialBreakpoints)
        {
            if (initialBreakpoints == null) return;

            Log.InfoFormat("ScriptDebugger: Initial Breakpoints: {0}", initialBreakpoints.Count());
            ClearBreakpoints();

            foreach (var bp in initialBreakpoints)
            {
                SetBreakpoint(bp);
                _breakpoints.Add(bp);

                enum_BP_STATE[] pState = new enum_BP_STATE[1];
                if (bp.GetState(pState) == VSConstants.S_OK)
                {
                    if (pState[0] == enum_BP_STATE.BPS_DISABLED)
                    {
                        EnableBreakpoint(bp, 0);  // Disable PS breakpoint
                    }
                }
            }
        }

        /// <summary>
        /// Placeholder for future support on debugging command in REPL window
        /// Breakpoint has been updated
        /// </summary>
        /// <param name="e"></param>
        public void UpdateBreakpoint(DebuggerBreakpointUpdatedEventArgs e)
        {
            Log.InfoFormat("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (BreakpointUpdated != null)
            {
                BreakpointUpdated(this, e);
            }
        }

        /// <summary>
        /// Process line breakpoint when break
        /// </summary>
        /// <param name="script">Script file</param>
        /// <param name="line">Line of breakpoint</param>
        /// <param name="column">Column of breakpoint</param>
        /// <returns>Success or not</returns>
        public bool ProcessLineBreakpoints(string script, int line, int column)
        {
            Log.InfoFormat("Process Line Breapoints");

            var bp =
                _breakpoints.FirstOrDefault(
                    m =>
                    line == m.Line &&
                    script.Equals(m.File, StringComparison.InvariantCultureIgnoreCase));

            if (bp != null)
            {
                if (BreakpointHit != null)
                {
                    Log.InfoFormat("Breakpoint @ {0} {1} {2} was hit.", bp.File, bp.Line, bp.Column);
                    BreakpointHit(this, new EventArgs<ScriptBreakpoint>(bp));
                    bp.IncrementHitCount();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add breakpoint
        /// </summary>
        /// <param name="breakpoint">Breakpoint to be added</param>
        public void SetBreakpoint(ScriptBreakpoint breakpoint)
        {
            Log.InfoFormat("SetBreakpoint: {0} {1} {2}", breakpoint.File, breakpoint.Line, breakpoint.Column);
            string fileName = Debugger.DebuggingService.GetTrueFileName(breakpoint.File);

            try
            {
                if (Debugger.DebuggingService.GetRunspaceAvailability() == RunspaceAvailability.Available)
                {
                    Debugger.DebuggingService.SetBreakpoint(new PowerShellBreakpoint(fileName, breakpoint.Line, breakpoint.Column));
                }
                else if (Debugger.IsDebuggingCommandReady)
                {
                    Debugger.DebuggingService.ExecuteDebuggingCommandOutNull(string.Format(DebugEngineConstants.SetPSBreakpoint, fileName, breakpoint.Line));
                }
                _breakpoints.Add(breakpoint);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set breakpoint.", ex);
            }
        }

        /// <summary>
        /// Enable breakpoint
        /// </summary>
        /// <param name="breakpoint">Breakpoint to be added</param>
        /// <param name="fEnable">0 - disable, 1 - enable</param>
        public void EnableBreakpoint(ScriptBreakpoint breakpoint, int fEnable)
        {
            string operation = fEnable == 0 ? "Disable" : "Enable";

            Log.InfoFormat("{3} breakpoint: {0} {1} {2}", breakpoint.File, breakpoint.Line, breakpoint.Column, operation);
            string fileName = Debugger.DebuggingService.GetTrueFileName(breakpoint.File);

            try
            {
                if (Debugger.DebuggingService.GetRunspaceAvailability() == RunspaceAvailability.Available)
                {
                    Debugger.DebuggingService.EnableBreakpoint(new PowerShellBreakpoint(fileName, breakpoint.Line, breakpoint.Column), fEnable == 0 ? false : true);
                }
                else if (Debugger.IsDebuggingCommandReady)
                {
                    int id = Debugger.DebuggingService.GetPSBreakpointId(new PowerShellBreakpoint(fileName, breakpoint.Line, breakpoint.Column));
                    if (id >= 0)
                    {
                        Debugger.DebuggingService.ExecuteDebuggingCommandOutNull(
                                fEnable == 0 ?
                                string.Format(DebugEngineConstants.DisablePSBreakpoint, id) :
                                string.Format(DebugEngineConstants.EnablePSBreakpoint, id));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to {0} breakpoint.", operation), ex);
            }
        }

        /// <summary>
        /// Remove breakpoint
        /// </summary>
        /// <param name="breakpoint">Breakpoint to be removed</param>
        public void RemoveBreakpoint(ScriptBreakpoint breakpoint)
        {
            Log.InfoFormat("RemoveBreakpoint: {0} {1} {2}", breakpoint.File, breakpoint.Line, breakpoint.Column);
            string fileName = Debugger.DebuggingService.GetTrueFileName(breakpoint.File);

            try
            {
                if (Debugger.DebuggingService.GetRunspaceAvailability() == RunspaceAvailability.Available)
                {
                    Debugger.DebuggingService.RemoveBreakpoint(new PowerShellBreakpoint(fileName, breakpoint.Line, breakpoint.Column));
                }
                else if (Debugger.IsDebuggingCommandReady)
                {
                    int id = Debugger.DebuggingService.GetPSBreakpointId(new PowerShellBreakpoint(fileName, breakpoint.Line, breakpoint.Column));
                    if (id >= 0)
                    {
                        Debugger.DebuggingService.RemoveBreakpointById(id);
                    }
                }
                _breakpoints.Remove(breakpoint);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to remove breakpoint.", ex);
            }
        }

        /// <summary>
        /// Clears existing breakpoints for the current runspace.
        /// </summary>
        public void ClearBreakpoints()
        {
            try
            {
                Debugger.DebuggingService.ClearBreakpoints();
                _breakpoints.Clear();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to clear existing breakpoints", ex);
            }
        }
    }
}
