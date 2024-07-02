using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;
using System;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// The Visual Studio implmentation of a breakpoint for PowerShell.
    /// </summary>
    public class ScriptBreakpoint : IDebugBoundBreakpoint2, IEnumDebugBoundBreakpoints2, IDebugPendingBreakpoint2, IDebugBreakpointResolution2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptBreakpoint));

        private readonly IEngineEvents _callback;
        private readonly ScriptProgramNode _node;
        private bool _enabled;
        private uint _hitCount;

        /// <summary>
        /// Line where this breakpoint is set.
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Column where this breakpoint is set.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Full path to the file this breakpoint is set within.
        /// </summary>
        public string File { get; private set; }

        public ScriptBreakpoint(ScriptProgramNode node, string file, int line, int column, IEngineEvents callback)
        {
            Log.InfoFormat("ScriptBreakPoint: {0} {1} {2}", file, line, column);

            _node = node;
            _callback = callback;
            _enabled = true;
            Line = line;
            Column = column;
            File = file;
            _hitCount = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                ScriptBreakpoint other = obj as ScriptBreakpoint;
                if (other != null)
                {
                    return other.Column == Column && other.Line == Line &&
                            File.Equals(other.File, StringComparison.InvariantCultureIgnoreCase) &&
                            _node.Equals(other._node);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _node.GetHashCode()
                ^ Line.GetHashCode()
                ^ Column.GetHashCode()
                ^ File.GetHashCode();
        }

        public void IncrementHitCount()
        {
            _hitCount++;
        }

        #region Implementation of IDebugBoundBreakpoint2

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            Log.Debug("ScriptBreakpoint: GetPendingBreakpoint");
            ppPendingBreakpoint = this;
            return VSConstants.S_OK;
        }

        public int GetState(enum_BP_STATE[] pState)
        {
            Log.Debug("ScriptBreakpoint: IDebugBoundBreakpoint2:GetState");
            pState[0] = _enabled ? enum_BP_STATE.BPS_ENABLED : enum_BP_STATE.BPS_DISABLED;
            return VSConstants.S_OK;
        }

        public int GetHitCount(out uint pdwHitCount)
        {
            Log.Debug("ScriptBreakpoint: GetHitCount");
            pdwHitCount = _hitCount;
            return VSConstants.S_OK;
        }

        public int GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            Log.Debug("ScriptBreakpoint: GetBreakpointResolution");
            ppBPResolution = this;
            return VSConstants.S_OK;
        }

        public int Enable(int fEnable)
        {
            Log.Debug("ScriptBreakpoint: Enable");
            _enabled = fEnable == 0 ? false : true;
            _callback.BreakpointEnabled(this, fEnable);
            return VSConstants.S_OK;
        }

        public int SetHitCount(uint dwHitCount)
        {
            Log.Debug("ScriptBreakpoint: SetHitCount");
            _hitCount = dwHitCount;
            return VSConstants.S_OK;
        }

        public int SetCondition(BP_CONDITION bpCondition)
        {
            Log.Debug("ScriptBreakpoint: SetCondition");
            return VSConstants.E_NOTIMPL;
        }

        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            Log.Debug("ScriptBreakpoint: SetPassCount");
            return VSConstants.E_NOTIMPL;
        }

        public int Delete()
        {
            Log.Debug("ScriptBreakpoint: Delete");
            _callback.BreakpointRemoved(this);
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IEnumDebugBoundBreakpoints2

        public int Next(uint celt, IDebugBoundBreakpoint2[] rgelt, ref uint pceltFetched)
        {
            Log.Debug("ScriptBreakpoint: Next");
            rgelt[0] = this;
            pceltFetched = 1;
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            Log.Debug("ScriptBreakpoint: Skip");
            return VSConstants.E_NOTIMPL;
        }

        public int Reset()
        {
            Log.Debug("ScriptBreakpoint: Reset");
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            Log.Debug("ScriptBreakpoint: Clone");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            Log.Debug("ScriptBreakpoint: GetCount");
            pcelt = 1;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugPendingBreakpoint2

        public int CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            Log.Debug("ScriptBreakpoint: CanBind");
            ppErrorEnum = null;
            return VSConstants.S_OK;
        }

        public int Bind()
        {
            Log.Debug("ScriptBreakpoint: Bind");
            _callback.Breakpoint(_node, this);
            return VSConstants.S_OK;
        }

        public int GetState(PENDING_BP_STATE_INFO[] pState)
        {
            Log.Debug("ScriptBreakpoint: IDebugPendingBreakpoint2:GetState");
            var state = new PENDING_BP_STATE_INFO
                            {
                                state = _enabled ? enum_PENDING_BP_STATE.PBPS_ENABLED : enum_PENDING_BP_STATE.PBPS_DISABLED,
                                Flags = enum_PENDING_BP_STATE_FLAGS.PBPSF_NONE
                            };

            pState[0] = state;
            return VSConstants.S_OK;
        }

        public int GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            Log.Debug("ScriptBreakpoint: GetBreakpointRequest");
            ppBPRequest = null;
            return VSConstants.S_OK;
        }

        public int Virtualize(int fVirtualize)
        {
            Log.Debug("ScriptBreakpoint: Virtualize");
            return VSConstants.S_OK;
        }

        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            Log.Debug("ScriptBreakpoint: EnumBoundBreakpoints");
            ppEnum = this;
            return VSConstants.S_OK;
        }

        public int EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            Log.Debug("ScriptBreakpoint: EnumErrorBreakpoints");
            ppEnum = null;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugBreakpointResolution2

        public int GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            Log.Debug("ScriptBreakpoint: GetBreakpointType");
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }


        public int GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            //VS line\column is zero based. PowerShell is 1
            var documentContext = new ScriptDocumentContext(File, "", Line - 1, Column);

            Log.Debug("ScriptBreakpoint: GetResolutionInfo");
            if (dwFields == enum_BPRESI_FIELDS.BPRESI_ALLFIELDS)
            {
                var loc = new BP_RESOLUTION_LOCATION
                              {
                                  bpType = (uint)enum_BP_TYPE.BPT_CODE,
                                  unionmember1 = Marshal.GetComInterfaceForObject(documentContext, typeof(IDebugCodeContext2))
                              };

                pBPResolutionInfo[0].bpResLocation = loc;
                pBPResolutionInfo[0].pProgram = _node;
                pBPResolutionInfo[0].pThread = _node;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}
