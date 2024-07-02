using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace PowerShellTools.DebugEngine
{
    public interface IRunspaceRequest
    {
        void SetRunspace(Runspace runspace, IEnumerable<PendingBreakpoint> pendingBreakpoints );
    }
}
