using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    internal class PowerShellProjectConfig : CommonProjectConfig
    {
        private readonly CommonProjectPackage _package;
        private readonly ProjectNode _projectNode;

        public PowerShellProjectConfig(CommonProjectPackage package, CommonProjectNode project, string configuration)
            : base(project, configuration)
        {
            _package = package;
            _projectNode = project;
        }
    }
}
