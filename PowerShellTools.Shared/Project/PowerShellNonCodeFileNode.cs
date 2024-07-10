using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    class PowerShellNonCodeFileNode : CommonNonCodeFileNode
    {
        public PowerShellNonCodeFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }


        public override int QueryService(ref Guid guidService, out object result)
        {
            //
            // If you have a code dom provider you'd provide it here.
            if (guidService == typeof(SVSMDCodeDomProvider).GUID)
            {
                result = new PowerShellCodeDomProvider();
                return VSConstants.S_OK;
            }

            return base.QueryService(ref guidService, out result);
        }
    }
}
