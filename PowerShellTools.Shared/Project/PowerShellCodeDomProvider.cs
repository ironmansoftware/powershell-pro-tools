using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Project
{
    class PowerShellCodeDomProvider : IVSMDCodeDomProvider
    {
        public object CodeDomProvider
        {
            get
            {
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var x = componentModel.DefaultExportProvider.GetExports<CodeDomProvider>("PowerShellCodeDomProvider").FirstOrDefault();

                if (x == null) return null;

                var value = x.Value;
                return value;
            }
        }
    }
}
