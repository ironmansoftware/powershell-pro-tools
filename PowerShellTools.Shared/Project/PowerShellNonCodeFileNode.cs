using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools.Project;
using Microsoft.Windows.Design.Host;
using PowerShellTools.Shared.WpfSupport;

namespace PowerShellTools.Project
{
    class PowerShellNonCodeFileNode : CommonNonCodeFileNode
    {
        private DesignerContext _designerContext;
        private PowerShellFileNode _child;

        public PowerShellNonCodeFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        protected internal object DesignerContext
        {
            get
            {
                if (_designerContext == null)
                {
                    _designerContext = new DesignerContext();
                    var child = ProjectMgr.FindNodeByFullPath(Url + PowerShellConstants.PS1File) as DependentFileNode;
                    if (child == null) return _designerContext;
                    _child = new PowerShellFileNode(ProjectMgr, child.ItemNode);
                    var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                    var x = componentModel.DefaultExportProvider.GetExports<Func<Func<IWpfTextView>, Func<IWpfTextView>, EventBindingProvider>>("WpfEventProviderFactory").FirstOrDefault();

                    //_designerContext.AssemblyReferenceProvider = new WpfAssemblyReferenceProvider();
                    _designerContext.EventBindingProvider = x.Value(GetTextView, _child.GetTextView);
                }
                return _designerContext;
            }
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

            if (guidService == typeof(DesignerContext).GUID && Path.GetExtension(Url)?.Equals(".xaml", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Create a DesignerContext for the XAML designer for this file
                result = DesignerContext;
                return VSConstants.S_OK;
            }

            return base.QueryService(ref guidService, out result);
        }
    }
}
