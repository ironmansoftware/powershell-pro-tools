using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace PowerShellTools.Project
{
    [Guid("F5034706-568F-408A-B7B3-4D38C6DB8A32")]
    public class PowerShellProjectFactory : ProjectFactory
    {
        private readonly CommonProjectPackage _package;
        public PowerShellProjectFactory(CommonProjectPackage package) : base(package)
        {
            this._package = package;

            var loc = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(loc);
            //TODO:
            var targetspath = Path.Combine(fileInfo.Directory.FullName, "PowerShellTools.Targets");

           // package.SetGlobalProperty("PowerGUIVSXTargets", targetspath);

            var taskpath = Path.Combine(fileInfo.Directory.FullName, "PowerShellTools.Targets");

            //BuildEngine.SetGlobalProperty("PowerGUIVSXTasks", loc);
        }

        internal override ProjectNode CreateProject()
        {
            var project = new PowerShellProjectNode(_package);
            project.SetSite((IServiceProvider)((System.IServiceProvider)this._package).GetService(typeof(IServiceProvider)));
            
            return project;
        }


    }
}
