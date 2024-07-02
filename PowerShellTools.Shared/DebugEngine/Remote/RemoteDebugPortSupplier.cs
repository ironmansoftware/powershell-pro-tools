using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerShellTools.Common;

namespace PowerShellTools.DebugEngine.Remote
{
    /// <summary>
    /// Supplies the PowerShell Tools remote debugging transport to the
    /// attach to process dialog.
    /// </summary>
    internal class RemoteDebugPortSupplier : IPowerShellToolsPortSupplier
    {
        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            ppPort = null;

            string pName;
            pRequest.GetPortName(out pName);
            ppPort = new RemoteDebugPort(this, pRequest, pName);

            return VSConstants.S_OK;
        }

        public bool UsesSSL()
        {
            return true;
        }

        public int CanAddPort()
        {
            return VSConstants.S_OK;
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.S_OK;   
        }

        public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText)
        {
            pbstrText = Resources.RemotePortDescription;
            return VSConstants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            ppPort = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier)
        {
            pguidPortSupplier = Constants.PortSupplierGuid;
            return VSConstants.S_OK;
        }

        public int GetPortSupplierName(out string pbstrName)
        {
            pbstrName = Resources.RemoteDebugTitle;
            return VSConstants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}
