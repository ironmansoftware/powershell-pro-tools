#region Usings

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.Logging;

#endregion

namespace PowerShellTools.DebugEngine
{
    // This class implements IDebugProgramNode2.
    // This interface represents a program that can be debugged.
    // A debug engine (DE) or a custom port supplier implements this interface to represent a program that can be debugged. 
    public class ScriptProgramNode : IDebugProgramNode2, IDebugProgram2, IDebugProgramNodeAttach2, IDebugEngineProgram2, IDebugThread2, IEnumDebugThreads2, IDebugModule3
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ScriptProgramNode));

        public ScriptDebugProcess Process { get; set; }
        public ScriptDebugger Debugger
        {
            get;
            set;
        }

        public Guid Id { get; set; }
        /// <summary>
        /// This is either the file name or the contents of the script. IsFile determines which it is. 
        /// TODO: Rename.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Arguments that will be passed to the script.
        /// </summary>
        public string Arguments { get; set; }
        /// <summary>
        /// Whether or not this program node represents a file or a command being debugged.
        /// </summary>
        public bool IsFile { get; set; }
        /// <summary>
        /// Whether or not this program node represents a runspace that is being debugged that has been
        /// attached to.
        /// </summary>
        public bool IsAttachedProgram { get; set; }
        /// <summary>
        /// Whether or not this program node represents a runspace on a remote machine.
        /// </summary>
        public bool IsRemoteProgram { get; set; }

        public ScriptProgramNode(ScriptDebugProcess process)
        {
            Id = Guid.NewGuid();
            this.Process = process;
        }

        #region IDebugProgramNode2 Members

        public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
        {
            Log.Debug("ScriptProgramNode: Entering GetHostName");
            pbstrHostName = null;
            return VSConstants.E_NOTIMPL;
        }

        // Gets the name and identifier of the DE running this program.
        int IDebugProgramNode2.GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            Log.Debug("ScriptProgramNode: Entering GetEngineInfo");
            engineName = ResourceStrings.EngineName;
            engineGuid = new Guid(Engine.Id);

            return VSConstants.S_OK;
        }

        // Gets the system process identifier for the process hosting a program.
        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            Log.Debug("ScriptProgramNode: Entering GetHostPid");
            // Return the process id of the debugged process
            pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            pHostProcessId[0].guidProcessId = Process.Id;

            return VSConstants.S_OK;
        }

        // Gets the name of a program.
        int IDebugProgramNode2.GetProgramName(out string programName)
        {
            Log.Debug("ScriptProgramNode: Entering GetProgramName");
            // Since we are using default transport and don't want to customize the process name, this method doesn't need
            // to be implemented.
            programName = null;
            return VSConstants.E_NOTIMPL;            
        }

        #endregion

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.DetachDebugger_V7()
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string hostMachineName)
        {
            Debug.Fail("This function is not called by the debugger");

            hostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugProgram2


        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            Log.Debug("Program: WriteDump");
            return VSConstants.E_NOTIMPL;
        }


        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            Log.Debug("ScriptProgramNode: Entering EnumThreads");
            ppEnum = this;
            return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            Log.Debug("ScriptProgramNode: Entering GetName");

            pbstrName = "PowerShell Script";
            return VSConstants.S_OK;
        }

        public int GetProcess(out IDebugProcess2 ppProcess)
        {
            Log.Debug("ScriptProgramNode: Entering GetProcess");
            ppProcess = Process;
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            Log.Debug("ScriptProgramNode: Entering Terminate");
            return VSConstants.S_OK;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            Log.Debug("ScriptProgramNode: Entering Attach");
            return VSConstants.S_OK;
        }

        public int CanDetach()
        {
            Log.Debug("ScriptProgramNode: Entering CanDetach");
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            Log.Debug("ScriptProgramNode: Entering Detach");

            DebugScenario scenario = Debugger.DebuggingService.GetDebugScenario();
            bool result = (scenario == DebugScenario.Local);

            if (scenario == DebugScenario.LocalAttach)
            {
                result = Debugger.DebuggingService.DetachFromRunspace();
            }
            else if (scenario == DebugScenario.RemoteAttach)
            {
                result = Debugger.DebuggingService.DetachFromRemoteRunspace();
            }

            if (!result)
            {
                // try as hard as we can to detach/cleanup the mess for the length of CleanupRetryTimeout
                TimeSpan retryTimeSpan = TimeSpan.FromMilliseconds(DebugEngineConstants.CleanupRetryTimeout);
                Stopwatch timeElapsed = Stopwatch.StartNew();
                while (timeElapsed.Elapsed < retryTimeSpan && !result)
                {
                    result = (Debugger.DebuggingService.CleanupAttach() == DebugScenario.Local);
                }
            }
            
            if (result)
            {
                Debugger.DebuggerFinished();
                Debugger.RefreshPrompt();
            }
        

            return result ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        public int GetProgramId(out Guid pguidProgramId)
        {
            Log.Debug(String.Format("ScriptProgramNode: Entering GetProgramId {0}", Id));
            pguidProgramId = Id;
            return VSConstants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            Log.Debug("ScriptProgramNode: Entering GetDebugProperty");
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            Log.Debug("ScriptProgramNode: Entering Execute");
            Debugger.Continue();
            return VSConstants.S_OK;
        }

        public int Continue(IDebugThread2 pThread)
        {
            Log.Debug("ScriptProgramNode: Entering Continue");
            Debugger.Continue();
            return VSConstants.S_OK;
        }

        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            Log.Debug("ScriptProgramNode: Entering Step");
            switch(sk)
            {
                case enum_STEPKIND.STEP_OVER:
                    Debugger.StepOver();
                    break;
                case enum_STEPKIND.STEP_INTO:
                   Debugger.StepInto();
                    break;
                case enum_STEPKIND.STEP_OUT:
                   Debugger.StepOut();
                    break;
            }
            return VSConstants.S_OK;
        }

        public int CauseBreak()
        {
            Log.Debug("ScriptProgramNode: Entering CauseBreak");
            //TODO: Debugger.DebuggerManager.BreakDebug();
            return VSConstants.S_OK;
        }

        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            Log.Debug("ScriptProgramNode: Entering GetEngineInfo");
            pbstrEngine = ResourceStrings.EngineName;
            pguidEngine = Guid.Parse(Engine.Id);
            return VSConstants.S_OK;
        }

        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            Log.Debug("Program: EnumCodeContexts");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            Log.Debug("Program: GetMemoryBytes");
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            Log.Debug("Program: GetDisassemblyStream");
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            Log.Debug("Program: EnumModules");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetENCUpdate(out object ppUpdate)
        {
            Log.Debug("Program: GetENCUpdate");
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            Log.Debug("Program: EnumCodePaths");
            ppEnum = null;
            ppSafety = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugProgramNodeAttach2

        public int OnAttach(ref Guid guidProgramId)
        {
            Log.Debug("ScriptProgramNode: Entering OnAttach");
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugEngineProgram2

        public int Stop()
        {
            Log.Debug("Program: Stop");
            Debugger.Stop();
            return VSConstants.S_OK;
        }

        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            Log.Debug("Program: WatchForThreadStep");
            return VSConstants.S_OK;
        }

        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            Log.Debug("Program: WatchForExpressionEvaluationOnThread");
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugThread2

        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            Log.Debug("Thread: EnumFrameInfo");
            ppEnum = new ScriptStackFrameCollection(Debugger.CallStack, this);
            return VSConstants.S_OK;
        }

        public int SetThreadName(string pszName)
        {
            Log.Debug("Thread: SetThreadName");
            return VSConstants.E_NOTIMPL;
        }

        public int GetProgram(out IDebugProgram2 ppProgram)
        {
            Log.Debug("Thread: GetProgram");
            ppProgram = this;
            return VSConstants.S_OK;
        }

        public int CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            Log.Debug("Thread: CanSetNextStatement");
            return VSConstants.S_OK;
        }

        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            Log.Debug("Thread: SetNextStatement");
            return VSConstants.S_OK;
        }

        public int GetThreadId(out uint pdwThreadId)
        {
            Log.Debug("Thread: GetThreadId");
            pdwThreadId = 0;
            return VSConstants.S_OK;
        }

        public int Suspend(out uint pdwSuspendCount)
        {
            Log.Debug("Thread: Suspend");
            pdwSuspendCount = 0;
            return VSConstants.S_OK;
        }

        public int Resume(out uint pdwSuspendCount)
        {
            Log.Debug("Thread: Resume");
            pdwSuspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {
            Log.Debug("Thread: GetThreadProperties");

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_ID) != 0)
            {
                ptp[0].dwThreadId = 0;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;    
            }

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0)
            {
                ptp[0].bstrName = "Thread";
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0)
            {
                ptp[0].dwThreadState = (int)enum_THREADSTATE.THREADSTATE_STOPPED;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }

            return VSConstants.S_OK;
        }

        public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            Log.Debug("Thread: GetLogicalThread");
            ppLogicalThread = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IEnumDebugThreads2

        public int Next(uint celt, IDebugThread2[] rgelt, ref uint pceltFetched)
        {
            rgelt[0] = this;
            pceltFetched = 1;
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugThreads2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = 1;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugModule2

        public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            Log.Debug("ScriptProgramNode: IDebugModule2.GetInfo");
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_NAME) != 0)
            {
                pinfo[0].m_bstrName = FileName;
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
            }

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_FLAGS) != 0)
            {
                pinfo[0].m_dwModuleFlags = enum_MODULE_FLAGS.MODULE_FLAG_SYMBOLS;
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
            }

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION) != 0)
            {
                pinfo[0].m_bstrUrlSymbolLocation = @".\";
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
            }

            return VSConstants.S_OK;
        }

        public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IDebugModule3

        public int GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            // This engine only supports loading symbols at the location specified in the binary's symbol path location in the PE file and
            // does so only for the primary exe of the debuggee.
            // Therefore, it only displays if the symbols were loaded or not. If symbols were loaded, that path is added.
            pinfo[0] = new MODULE_SYMBOL_SEARCH_INFO();
            pinfo[0].dwValidFields = 1; // SSIF_VERBOSE_SEARCH_INFO;

            string symbolPath = "Symbols Loaded - " + FileName;
            pinfo[0].bstrVerboseSearchInfo = symbolPath;

            return VSConstants.S_OK;
        }

        public int LoadSymbols()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int IsUserCode(out int pfUser)
        {
            pfUser = 1;
            return VSConstants.S_OK;
        }

        public int SetJustMyCodeState(int fIsUserCode)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }

}