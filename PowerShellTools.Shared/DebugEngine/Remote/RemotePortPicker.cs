using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    /// <summary>
    /// Class is utilized by Visual Studio when the Attach to Process dialog is active and the PowerShell Tools
    /// Remote Debugging option is selected.
    /// </summary>
    internal class RemotePortPicker : IDebugPortPicker
    {
        public int DisplayPortPicker(IntPtr hwndParentDialog, out string pbstrPortId)
        {
            pbstrPortId = null;
            return VSConstants.E_NOTIMPL;
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider pSP)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}
