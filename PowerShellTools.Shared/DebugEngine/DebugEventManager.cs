using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using PowerShellTools.DebugEngine;

namespace PowerShellTools
{
    /// <summary>
    ///     Manages events coming from the debug engine.
    /// </summary>
    public class DebugEventManager : IVsDebuggerEvents, IDebugEventCallback2
    {
        private Runspace _runspace;
        private List<PendingBreakpoint> _breakpoints;

        public DebugEventManager(Runspace runspace)
        {
            _runspace = runspace;
            _breakpoints = new List<PendingBreakpoint>();

            var dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

            foreach (Breakpoint bp in dte2.Debugger.Breakpoints)
            {
                if (bp.File.ToLower().EndsWith(".ps1"))
                {
                    var pbp = new PendingBreakpoint
                                  {
                                      BreakpointType = BreakpointType.Line,
                                      Column = bp.FileColumn,
                                      Line = bp.FileLine,
                                      Context = bp.File,
                                      Language = bp.Language
                                  };

                    _breakpoints.Add(pbp);
                }
            }
        }

        #region Methods

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            return VSConstants.S_OK;
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            if (pEvent is IRunspaceRequest)
            {
                var request = pEvent as IRunspaceRequest;
                request.SetRunspace(_runspace, _breakpoints);
            }

            return VSConstants.S_OK;
        }



        #endregion
    }
}
