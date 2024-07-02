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
    /// Interface for all remote debugging port suppliers that this tool implements. Enforces the
    /// need for said port suppliers to specify whether or not the -UseSSL parameter should be used
    /// when entering into a remote session with a machine.
    /// </summary>
    interface IPowerShellToolsPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2
    {
        bool UsesSSL();
    }
}
