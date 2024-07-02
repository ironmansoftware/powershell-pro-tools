using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Windows.Design.Host;
using PowerShellTools.Classification;
using PowerShellTools.DebugEngine;
using PowerShellTools.Shared.WpfSupport;

namespace PowerShellTools.Project {
    /// <summary>
    /// Provides access to the DesignerContext and WpfEventBindingProvider assuming that functionality
    /// is installed into VS.  If it's not installed then this becomes a nop and DesignerContextType
    /// returns null;
    /// </summary>
    class XamlDesignerSupport {
        private static readonly Type _designerContextType;

        static XamlDesignerSupport() {
            try {
                _designerContextType = GetDesignerContextType();
            } catch (FileNotFoundException) {
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetDesignerContextType() {
            return typeof(DesignerContext);
        }

        public static object CreateDesignerContext() {
            if (_designerContextType != null) {
                return CreateDesignerContextNoInline();
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object CreateDesignerContextNoInline() {
            var res = new DesignerContext();
            //Set the RuntimeNameProvider so the XAML designer will call it when items are added to
            //a design surface. Since the provider does not depend on an item context, we provide it at 
            //the project level.
            // This is currently disabled because we don't successfully serialize to the remote domain
            // and the default name provider seems to work fine.  Likely installing our assembly into
            // the GAC or implementing an IsolationProvider would solve this.
            //res.RuntimeNameProvider = new PythonRuntimeNameProvider();
            return res;
        }

        public static void InitializeEventBindingProvider(object designerContext, PowerShellFileNode codeNode) {
            if (_designerContextType != null) {
                InitializeEventBindingProviderNoInline(designerContext, codeNode);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeEventBindingProviderNoInline(object designerContext, PowerShellFileNode codeNode) {
            Debug.Assert(designerContext is DesignerContext);

	        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
	        var x = componentModel.DefaultExportProvider.GetExports<Func<Func<IWpfTextView>, EventBindingProvider>>("WpfEventProviderFactory").FirstOrDefault();

            //((DesignerContext)designerContext).AssemblyReferenceProvider = new WpfAssemblyReferenceProvider();
            ((DesignerContext) designerContext).EventBindingProvider = x.Value(codeNode.GetTextView);
        }

        public static Type DesignerContextType {
            get {
                return _designerContextType;
            }
        }
    }
}
