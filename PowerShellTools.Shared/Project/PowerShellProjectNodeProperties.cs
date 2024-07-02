using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(CommonConstants.ProjectNodePropertiesGuid)]
    public class PowerShellProjectNodeProperties : CommonProjectNodeProperties
    {
        internal PowerShellProjectNodeProperties(ProjectNode node)
            : base(node)
        {
        }

        //public new string StartupFile { get; set; }
    }
}