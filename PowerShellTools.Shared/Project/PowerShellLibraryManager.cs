using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;

namespace PowerShellTools.Project
{
    internal class PowerShellLibraryManager : LibraryManager
    {
        public PowerShellLibraryManager(CommonPackage package) : base(package)
        {
        }

        protected override LibraryNode CreateLibraryNode(LibraryNode parent, IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy,
            uint itemid)
        {
            Trace.WriteLine("PowerShellLibraryManager.CreateLibraryNode");
            return null;
        }
    }
}
