using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.ComponentsPropertiesPageGuid)]
    public class ModulePropertyPage : CommonPropertyPage
    {
        private readonly ModulePropertyPageControl _control;

        public ModulePropertyPage()
        {
            _control = new ModulePropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }

        public override void Apply()
        {
            Project.SetProjectProperty("FormatsToProcess", _control.FormatsToProcess);
            Project.SetProjectProperty("FunctionsToProcess", _control.FunctionsToProcess);
           
            Project.SetProjectProperty("ModuleList", _control.ModuleList);
            Project.SetProjectProperty("ModuleToProcess", _control.ModulesToProcess);

            Project.SetProjectProperty("NestedModules", _control.NestedModules);
            Project.SetProjectProperty("ScriptsToProcess", _control.ScriptsToProcess);
            Project.SetProjectProperty("TypesToProcess", _control.TypesToProcess);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;

            _control.FormatsToProcess = Project.GetProjectProperty("FormatsToProcess", true);
            _control.FunctionsToProcess = Project.GetProjectProperty("FunctionsToProcess", true);
            _control.ModuleList = Project.GetProjectProperty("ModuleList", true);
            _control.ModulesToProcess = Project.GetProjectProperty("ModuleToProcess", true);
            _control.NestedModules = Project.GetProjectProperty("NestedModules", true);
            _control.ScriptsToProcess = Project.GetProjectProperty("ScriptsToProcess", true);
            _control.TypesToProcess = Project.GetProjectProperty("TypesToProcess", true);
            
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Components"; }
        }
    }
}
