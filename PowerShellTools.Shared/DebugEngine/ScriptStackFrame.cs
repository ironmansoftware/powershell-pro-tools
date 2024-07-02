using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;


namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// Visual Studio implementation of a stack frame. This is needed for call stack support.
    /// </summary>
    public class ScriptStackFrame : IDebugStackFrame2, IDebugExpressionContext2
    {
        private readonly ScriptDebugger _debugger;
        private readonly ScriptProgramNode _node;
        private readonly ScriptDocumentContext _docContext;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptStackFrame));


        public ScriptStackFrame(ScriptProgramNode node, string scriptName, string frame, int startLine, int endLine, int startColumn, int endColumn)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            _node = node;
            _debugger = node.Debugger;
            // VS is zero based, PS is 1 based
            _docContext = new ScriptDocumentContext(scriptName, frame, startLine - 1, endLine - 1, startColumn - 1, endColumn - 1);
        }

        #region Implementation of IDebugStackFrame2

        public int GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
        {
            Log.Debug("ScriptStackFrame: GetCodeContext");
            ppCodeCxt = _docContext;
            return VSConstants.S_OK;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppCxt)
        {
            Log.Debug("ScriptStackFrame: GetDocumentContext");
            ppCxt = _docContext;
            return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            Log.Debug("ScriptStackFrame: GetName");
            pbstrName = _docContext.ToString();
            return VSConstants.S_OK;
        }

        public int GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            var frameInfo = pFrameInfo[0];

            Log.Debug("ScriptStackFrame: GetInfo");


            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME) != 0)
            {
                frameInfo.m_bstrFuncName = this.ToString();
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
            }

            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_MODULE) != 0)
            {
                frameInfo.m_pModule = _node;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
            }

            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP) != 0)
            {
                frameInfo.m_pModule = _node;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
            }


            // The debugger is requesting the IDebugStackFrame2 value for this frame info.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) != 0)
            {
                frameInfo.m_bstrModule = "Module";
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE;
            }

            // The debugger is requesting the IDebugStackFrame2 value for this frame info.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FRAME) != 0)
            {
                frameInfo.m_pFrame = this;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
            }

            // Does this stack frame of symbols loaded?
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO) != 0)
            {
                frameInfo.m_fHasDebugInfo = 1;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
            }

            // Is this frame stale?
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_STALECODE) != 0)
            {
                frameInfo.m_fStaleCode = 0;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
            }

            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_LANGUAGE) != 0)
            {
                frameInfo.m_bstrLanguage = "PowerShell";
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_LANGUAGE;
            }

            pFrameInfo[0] = frameInfo;

            return VSConstants.S_OK;
        }

        public int GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            Log.Debug("ScriptStackFrame: GetPhysicalStackRange");
            paddrMin = 0;
            paddrMax = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            Log.Debug("ScriptStackFrame: GetExpressionContext");
            ppExprCxt = this;
            return VSConstants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            Log.Debug("ScriptStackFrame: GetLanguageInfo");
            pguidLanguage = Guid.Empty;
            pbstrLanguage = "PowerShell";
            return VSConstants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            Log.Debug("ScriptStackFrame: GetDebugProperty");
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// This method returns the collection of variables that are exposed at this current call stack. This
        /// is used for the locals and watch windows.
        /// </summary>
        /// <param name="dwFields"></param>
        /// <param name="nRadix"></param>
        /// <param name="guidFilter"></param>
        /// <param name="dwTimeout"></param>
        /// <param name="pcelt"></param>
        /// <param name="ppEnum"></param>
        /// <returns></returns>
        public int EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            Log.Debug("ScriptStackFrame: EnumProperties");
            ppEnum = new ScriptPropertyCollection((skip, take) => _debugger.Variables.Skip((int)skip).Take((int)take).Select(m => new ScriptProperty(_debugger, m.Value)), (uint)_debugger.Variables.Count);
            ppEnum.GetCount(out pcelt);
            return VSConstants.S_OK;
        }

        public int GetThread(out IDebugThread2 ppThread)
        {
            Log.Debug("ScriptStackFrame: GetThread");
            ppThread = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        /// <summary>
        /// This method allows us to hover variables and view their values when stopped in the debugger.
        /// </summary>
        /// <param name="pszCode"></param>
        /// <param name="dwFlags"></param>
        /// <param name="nRadix"></param>
        /// <param name="ppExpr"></param>
        /// <param name="pbstrError"></param>
        /// <param name="pichError"></param>
        /// <returns></returns>
        public int ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError,
            out uint pichError)
        {
            var variable = _debugger.GetVariable(pszCode);

            pbstrError = null;
            pichError = 0;

            ppExpr = new ScriptExpression(_debugger, variable);

            return VSConstants.S_OK;
        }

        public override string ToString()
        {
            return _docContext.ToString();
        }
    }

    public class ScriptStackFrameCollection : List<ScriptStackFrame>, IEnumDebugFrameInfo2
    {
        private uint _iterationLocation;
        private readonly ScriptProgramNode _node;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptStackFrameCollection));

        public ScriptStackFrameCollection(IEnumerable<ScriptStackFrame> frames, ScriptProgramNode node)
        {
            foreach (var frame in frames)
            {
                Add(frame);
            }

            _iterationLocation = 0;
            _node = node;
        }

        #region Implementation of IEnumDebugFrameInfo2

        public int Next(uint celt, FRAMEINFO[] rgelt, ref uint pceltFetched)
        {
            pceltFetched = 0;

            if (_iterationLocation == Count) return VSConstants.S_FALSE;
            if (celt == 0) return VSConstants.S_OK;

            var currentIteration = _iterationLocation;

            for (uint i = currentIteration; i < currentIteration + celt; i++)
            {
                var currentFrame = this[(int)i];

                var index = i - currentIteration;

                Log.Debug("ScriptStackFrameCollection: Next");
                rgelt[index].m_dwValidFields = (enum_FRAMEINFO_FLAGS.FIF_LANGUAGE | enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO | enum_FRAMEINFO_FLAGS.FIF_STALECODE | enum_FRAMEINFO_FLAGS.FIF_FRAME | enum_FRAMEINFO_FLAGS.FIF_FUNCNAME | enum_FRAMEINFO_FLAGS.FIF_MODULE);
                rgelt[index].m_fHasDebugInfo = 1;
                rgelt[index].m_fStaleCode = 0;
                rgelt[index].m_bstrLanguage = "PowerShell";
                rgelt[index].m_bstrFuncName = currentFrame.ToString();
                rgelt[index].m_pFrame = currentFrame;
                rgelt[index].m_pModule = _node;

                pceltFetched++;
                _iterationLocation++;
            }

            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            Log.Debug("ScriptStackFrameCollection: Skip");
            _iterationLocation += celt;
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            Log.Debug("ScriptStackFrameCollection: Reset");
            _iterationLocation = 0;
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugFrameInfo2 ppEnum)
        {
            Log.Debug("ScriptStackFrameCollection: Clone");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            Log.Debug("ScriptStackFrameCollection: GetCount");
            pcelt = (uint)Count;
            return VSConstants.S_OK;
        }

        #endregion


    }

    public class ScriptExpression : IDebugExpression2, IDebugEventCallback2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptExpression));
        private readonly Variable _var;
        private readonly ScriptDebugger _debugger;

        public ScriptExpression(ScriptDebugger debugger, Variable var)
        {
            if (var != null)
            {
                Log.DebugFormat("Name [{0}] Value [{1}]", var.VarName, var.VarValue);

                if (var.VarName.StartsWith("$"))
                {
                    var.VarName = var.VarName.Remove(0, 1);
                }

                _var = var;
            }
            _debugger = debugger;
        }

        public int EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            Log.Debug("EvaluateAsync");
            throw new NotImplementedException();
        }

        public int Abort()
        {
            Log.Debug("Abort");
            return VSConstants.S_OK;
        }

        public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback,
            out IDebugProperty2 ppResult)
        {
            Log.Debug("EvaluateSync");

            if (_var == null)
            {
                ppResult = null;
                return VSConstants.E_INVALIDARG;
            }

            ppResult = new ScriptProperty(_debugger, _var); // no parent path
            return VSConstants.S_OK;
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread,
            IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            Log.Debug("Event");
            return VSConstants.E_NOTIMPL;
        }
    }
}
