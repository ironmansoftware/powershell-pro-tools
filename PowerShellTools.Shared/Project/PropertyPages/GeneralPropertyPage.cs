using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.GeneralPropertiesPageGuid)]
    public class GeneralPropertyPage : CommonPropertyPage
    {
        private PowerShellProjectNode _project;
        private readonly GeneralPropertyPageControl _control;

        public GeneralPropertyPage()
        {
            _control = new GeneralPropertyPageControl(this);
        }

        internal override CommonProjectNode Project
        {
            get { return _project;  }
            set { _project = (PowerShellProjectNode)value; }
        }

        public override Control Control
        {
            get
            {
                return _control;
            }
        }

        public override void Apply()
        {
            Project.SetProjectProperty(ProjectConstants.OutputDirectory, _control.OutputDirectory);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.OutputDirectory = Project.GetProjectProperty(ProjectConstants.OutputDirectory, false);
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "General"; }
        }
    }
}
