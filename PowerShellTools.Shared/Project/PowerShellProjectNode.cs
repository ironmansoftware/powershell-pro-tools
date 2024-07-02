using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;
using PowerShellTools.Project.Images;
using PowerShellTools.Project.PropertyPages;

namespace PowerShellTools.Project
{
    internal class PowerShellProjectNode : CommonProjectNode
    {
        private readonly CommonProjectPackage _package;
        private static readonly ImageList ProjectImageList =
            Utilities.GetImageList(
                typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream(
                    "PowerShellTools.Project.Resources.ImageList.bmp"));

        public PowerShellProjectNode(CommonProjectPackage package)
            : base(package, ProjectImageList)
        {
            base.CanProjectDeleteItems = true;
            _package = package;
            //AddCATIDMapping(typeof(GeneralPropertyPage), typeof(GeneralPropertyPage).GUID);
            AddCATIDMapping(typeof(DebugPropertyPage), typeof(DebugPropertyPage).GUID);
            AddCATIDMapping(typeof(ModuleManifestPropertyPage), typeof(ModuleManifestPropertyPage).GUID);
            AddCATIDMapping(typeof(BuildEventPropertyPage), typeof(BuildEventPropertyPage).GUID);
            AddCATIDMapping(typeof(AdvancedPropertyPage), typeof(AdvancedPropertyPage).GUID);
        }

        public override Type GetProjectFactoryType()
        {
            return typeof (PowerShellProjectFactory);
        }

        public override Type GetEditorFactoryType()
        {
            return typeof(PowerShellEditorFactory);
        }

        public override string GetProjectName()
        {
            return "PowerShellProject";
        }

        public override string GetFormatList()
        {
            return "PowerShell Project File (*.pssproj)\n*.pssproj\nAll Files (*.*)\n*.*\n";
        }

        public override Type GetGeneralPropertyPageType()
        {
            return null;
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            return new[] {
               // typeof(GeneralPropertyPage).GUID,
                typeof(DebugPropertyPage).GUID,
                typeof(ModuleManifestPropertyPage).GUID,
                typeof(BuildEventPropertyPage).GUID,
                typeof(AdvancedPropertyPage).GUID,};
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(PowerShellLibraryManager);
        }

        public override IProjectLauncher GetLauncher()
        {
            return new PowerShellProjectLauncher(this);
        }

        protected override Stream ProjectIconsImageStripStream
        {
            get
            {
                return typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream("PowerShellTools.Project.Resources.CommonImageList.bmp");
            }
        }

        public override string[] CodeFileExtensions
        {
            get
            {
                return new[] { PowerShellConstants.PS1File, PowerShellConstants.PSD1File, PowerShellConstants.PSM1File };
            }
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item)
        {
            var node = new PowerShellFileNode(this, item);
            return node;
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item)
        {
            var node = new PowerShellNonCodeFileNode(this, item);
            return node;
        }

        internal override string IssueTrackerUrl { get;  }

        protected override bool SupportsIconMonikers
        {
            get { return true; }
        }

        protected override ImageMoniker GetIconMoniker(bool open)
        {
            return KnownMonikers.FSApplication;
            //return PowerShellMonikers.ProjectIconImageMoniker;
        }

		public override int ImageIndex
        {
            get
            {
                return (int)ImageListIndex.Project;
            }
        }
        protected override ConfigProvider CreateConfigProvider()
        {
            return new PowerShellConfigProvider(_package, this);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new PowerShellProjectNodeProperties(this);
        }

        /// <summary>
        /// Creates the services exposed by this project.
        /// </summary>
        protected override object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(SVSMDCodeDomProvider) == serviceType)
            {
                service = new PowerShellCodeDomProvider();
            }

            return service;
        }

        public override bool IsCodeFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return false;

            var fi = new FileInfo(fileName);

            return CodeFileExtensions.Any(x => x.Equals(fi.Extension, StringComparison.OrdinalIgnoreCase));
        }

	    //public override DependentFileNode CreateDependentFileNode(MsBuildProjectElement item)
	    //{
		   // return new PowerShellFileNode(this, item);
	    //}

	}
}
