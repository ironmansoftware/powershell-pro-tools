using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudioTools.Project;
using PowerGUIVsx.Project;

namespace PowerShellTools.Project
{
    internal class PowerShellConfigProvider : CommonConfigProvider
    {
        private CommonProjectNode _node;
        private CommonProjectPackage _package;

        public PowerShellConfigProvider(CommonProjectPackage package, CommonProjectNode manager)
            : base(manager)
        {
            _package = package;
            _node = manager;
        }



        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            return new PowerShellProjectConfig(_package, _node, configName);
        }




    }
}
