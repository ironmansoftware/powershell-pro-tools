using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.BuildEventsPropertiesPageGuid)]
    public class BuildEventPropertyPage : CommonPropertyPage
    {
        private readonly BuildEventPropertyPageControl _control;

        public BuildEventPropertyPage()
        {
            _control = new BuildEventPropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }
        public override void Apply()
        {
            Project.SetProjectProperty("PreBuildScript", _control.Prebuild);
            Project.SetProjectProperty("PostBuildScript", _control.Postbuild);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.Prebuild = Project.GetUnevaluatedProperty("PrebuildScript");
            _control.Postbuild = Project.GetUnevaluatedProperty("PostbuildScript");
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Build Events"; }
        }
    }
}
