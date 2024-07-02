using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    /// <summary>
    /// Implementation of IDebugPort 2 interface. Contacts a remote machine and uses the IEnumDebugProcess2
    /// interface to grab all attachable processes off of that machine. 
    /// </summary>
    internal class RemoteDebugPort : IDebugPort2
    {
        private readonly IPowerShellToolsPortSupplier _supplier;
        private readonly IDebugPortRequest2 _request;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly string _computerName;

        public RemoteDebugPort(IPowerShellToolsPortSupplier supplier, IDebugPortRequest2 request, string computerName)
        {
            _supplier = supplier;
            _request = request;
            _computerName = computerName;
        }

        public string ComputerName
        {
            get { return _computerName; }
        }

        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            RemoteEnumDebugProcess processList = new RemoteEnumDebugProcess(_computerName);
            processList.connect(this, _supplier.UsesSSL());
            ppEnum = processList;
            return VSConstants.S_OK;
        }

        public int GetPortId(out Guid pguidPort)
        {
            pguidPort = _guid;
            return VSConstants.S_OK;
        }

        public int GetPortName(out string pbstrName)
        {
            pbstrName = _computerName.ToString();
            return VSConstants.S_OK;
        }

        public int GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            ppRequest = _request;
            return VSConstants.S_OK;
        }

        public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            ppSupplier = _supplier;
            return VSConstants.S_OK;
        }

        public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            RemoteEnumDebugProcess processList = new RemoteEnumDebugProcess(_computerName);
            processList.connect(this, _supplier.UsesSSL());
            uint numProcesses = 0;
            uint numRetrieved = 0;
            ppProcess = null;

            processList.GetCount(out numProcesses);
            IDebugProcess2[] processes = new IDebugProcess2[numProcesses];
            processList.Next(numProcesses, processes, ref numRetrieved);
            
            while (numRetrieved >= 0)
            {
                if ((processes[numRetrieved - 1] as ScriptDebugProcess).ProcessId == ProcessId.dwProcessId)
                {
                    ppProcess = processes[numRetrieved - 1];
                    return VSConstants.S_OK;
                }
                numRetrieved--;
            }

            return VSConstants.S_FALSE;
        }
    }
}
