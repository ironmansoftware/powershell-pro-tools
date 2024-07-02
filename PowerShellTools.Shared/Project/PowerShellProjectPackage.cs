using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;
using PowerShellTools.Common;
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.IO;

namespace PowerShellTools.Project
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(GuidList.PowerShellToolsProjectPackageGuid)]
    [ProvideProjectFactory(typeof(PowerShellProjectFactory), "PowerShell", "PowerShell Project Files (*.pssproj);*.pssproj", "pssproj", "pssproj", @"\ProjectTemplates\PowerShell", LanguageVsTemplate = "PowerShell", NewProjectRequireNewFolderVsTemplate = false)]
    [ProvideProjectItem(typeof(PowerShellProjectFactory), "PowerShell", @"Templates", 500)]
    [ProvideEditorExtension(typeof(PowerShellEditorFactory), PowerShellConstants.PSD1File, 1000)]
    [ProvideEditorExtension(typeof(PowerShellEditorFactory), PowerShellConstants.PS1File, 1000)]
    [ProvideEditorExtension(typeof(PowerShellEditorFactory), PowerShellConstants.PSM1File, 1000)]
    [ProvideEditorLogicalView(typeof(PowerShellEditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Designer
    [ProvideEditorLogicalView(typeof(PowerShellEditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Code
    [DeveloperActivity("PowerShell", typeof(PowerShellProjectPackage))]
    [Export]
    public class PowerShellProjectPackage : CommonProjectPackage
    {
        public PowerShellProjectPackage()
        {
            UiContextUtilities.ActivateUiContext(UiContextUtilities.CreateUiContext(PowerShellTools.Common.Constants.PowerShellProjectUiContextGuid));
        }

        public override ProjectFactory CreateProjectFactory()
        {
            return new PowerShellProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactory()
        {
            return new PowerShellEditorFactory(this);
        }

        public override uint GetIconIdForAboutBox()
        {
            //TODO: GetIconIdForAboutBox
            return 0;
        }

        public override uint GetIconIdForSplashScreen()
        {
            //TODO: GetIconIdFroSplashScreen
            return 0;
        }

        public override string GetProductName()
        {
            return PowerShellConstants.LanguageName;
        }

        public override string GetProductDescription()
        {
            return PowerShellConstants.LanguageName;
        }

        public override string GetProductVersion()
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}
