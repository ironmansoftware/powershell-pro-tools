using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.DebugPropertiesPageGuid)]
    public class DebugPropertyPage : CommonPropertyPage
    {
        private readonly DebugPropertyPageControl _control;

        public DebugPropertyPage()
        {
            _control = new DebugPropertyPageControl(this);
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
            Project.SetProjectProperty(ProjectConstants.DebugArguments, _control.Arguments);
            Project.SetProjectProperty(CommonConstants.StartupFile, _control.Script);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.Arguments = Project.GetUnevaluatedProperty(ProjectConstants.DebugArguments);
            _control.Script = Project.GetUnevaluatedProperty(CommonConstants.StartupFile);
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Debug"; }
        }
    }
}
