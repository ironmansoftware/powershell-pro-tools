using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common.Logging;
using PowerShellTools.DebugEngine.Remote;

namespace PowerShellTools.DebugEngine
{
    public class ScriptDebugProcess : IDebugProcess2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptDebugProcess));

        private readonly IDebugPort2 _port;
        private string _processName;

        public ScriptDebugProcess(IDebugPort2 debugPort, uint processId, string processName, string hostname) : this(debugPort, processId)
        {
            _processName = processName;
            HostName = hostname;
        }

        public ScriptDebugProcess(IDebugPort2 debugPort, uint processId) : this(debugPort)
        {
            ProcessId = processId;
            if (_processName == null)
            {
                _processName = string.Empty;
            }
        }

        public ScriptDebugProcess(IDebugPort2 debugPort)
        {
            Log.Debug("Process: Constructor");
            Id = Guid.NewGuid();
            _port = debugPort;
            Node = new ScriptProgramNode(this);
        }

        public uint ProcessId { get; set; }

        public Guid Id { get; set; }

        public ScriptProgramNode Node { get; set; }

        public string HostName { get; set; }

        public int GetInfo(enum_PROCESS_INFO_FIELDS fields, PROCESS_INFO[] pProcessInfo)
        {
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME) != 0)
            {
                pProcessInfo[0].bstrFileName = Node.FileName;
                pProcessInfo[0].Flags = enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING;

                if (Node.Debugger != null)
                {
                    pProcessInfo[0].Flags = pProcessInfo[0].Flags | enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED;
                }
                
                pProcessInfo[0].Fields = fields;
                pProcessInfo[0].bstrBaseName = _processName;
                pProcessInfo[0].bstrTitle = string.Empty;
                pProcessInfo[0].ProcessId.dwProcessId = ProcessId;
                pProcessInfo[0].dwSessionId = 0;
            }
            return VSConstants.S_OK;
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            Log.Debug("Process: EnumPrograms");
            ppEnum = new RemoteEnumDebugPrograms(this);
            return VSConstants.S_OK;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            Log.Debug("Process: GetName");
            pbstrName = "PowerShell Script Process";
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            Log.Debug("Process: GetServer");
            ppServer = null;
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            Log.Debug("Process: Terminate");
            return VSConstants.S_OK;
        }

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            Log.Debug("Process: Attach");
            return VSConstants.S_OK;
        }

        public int CanDetach()
        {
            Log.Debug("Process: CanDetach");
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            Log.Debug("Process: Detach");
            return VSConstants.S_OK;
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            Log.Debug("Process: GetPhysicalProcessId");
            var pidStruct = new AD_PROCESS_ID();

            // if dealing with an actual process we need to identify it with its PID
            if(ProcessId != 0)
            {
                pidStruct.dwProcessId = (uint)ProcessId;
                pidStruct.ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            }
            else
            {
                pidStruct.guidProcessId = Id;
                pidStruct.ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            }
            
            pProcessId[0] = pidStruct;

            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            Log.Debug("Process: GetProcessId");
            pguidProcessId = Id;
            return VSConstants.S_OK;
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            Log.Debug("Process: GetAttachedSessionName");
            pbstrSessionName = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            Log.Debug("Process: EnumThreads");
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int CauseBreak()
        {
            Log.Debug("Process: CauseBreak");
            return VSConstants.S_OK;
        }

        public int GetPort(out IDebugPort2 ppPort)
        {
            Log.Debug("Process: GetPort");
            ppPort = _port;
            return VSConstants.S_OK;
        }

        public int QueryCanSafelyAttach()
        {
            return VSConstants.S_OK;
        }

        public int GetUserName(out string pbstrUserName)
        {
            pbstrUserName = string.Empty;
            return VSConstants.S_OK;
        }
    }
}
