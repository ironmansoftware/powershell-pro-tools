using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.ModuleManifestPropertiesPageGuid)]
    public class ModuleManifestPropertyPage : CommonPropertyPage
    {
        private readonly ModuleManifestPropertyPageControl _control;

        public ModuleManifestPropertyPage()
        {
            _control = new ModuleManifestPropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }

        public override void Apply()
        {
            Project.SetProjectProperty("ManifestPath", _control.Path);

            Project.SetProjectProperty("FormatsToProcess", _control.FormatsToProcess);
            Project.SetProjectProperty("FunctionsToProcess", _control.FunctionsToProcess);
           
            Project.SetProjectProperty("ModuleList", _control.ModuleList);
            Project.SetProjectProperty("ModuleToProcess", _control.ModulesToProcess);

            Project.SetProjectProperty("NestedModules", _control.NestedModules);
            Project.SetProjectProperty("ScriptsToProcess", _control.ScriptsToProcess);
            Project.SetProjectProperty("TypesToProcess", _control.TypesToProcess);

            Project.SetProjectProperty("VariablesToExport", _control.VariablesToExport);
            Project.SetProjectProperty("CmdletsToExport", _control.CmdletsToExport);
            Project.SetProjectProperty("AliasesToExport", _control.AliasesToExport);

            Project.SetProjectProperty("Author", _control.Author);
            Project.SetProjectProperty("CompanyName", _control.Company);
            Project.SetProjectProperty("Copyright", _control.Copyright);
            Project.SetProjectProperty("Description", _control.Description);
            Project.SetProjectProperty("Version", _control.Version);
            Project.SetProjectProperty("Guid", _control.Guid);

            Project.SetProjectProperty("ClrVersion", _control.ClrVersion);
            Project.SetProjectProperty("PowerShellHostVersion", _control.PowerShellHostVersion);
            Project.SetProjectProperty("PowerShellVersion", _control.PowerShellVersion);
            Project.SetProjectProperty("ProcessorArchitecture", _control.ProcessorArchitecture);
            Project.SetProjectProperty("RequiredModules", _control.RequiredModules);
           // Project.SetProjectProperty("GenerateModuleManifest", _control.GenerateModuleManifest.ToString());

            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.Path = Project.GetUnevaluatedProperty("ManifestPath");
            if (string.IsNullOrEmpty(_control.Path))
            {
                _control.Path = "$(OutDir)\\$(ProjectName).psd1";
            }

            _control.FormatsToProcess = Project.GetUnevaluatedProperty("FormatsToProcess");
            _control.FunctionsToProcess = Project.GetUnevaluatedProperty("FunctionsToProcess");
            _control.ModuleList = Project.GetUnevaluatedProperty("ModuleList");
            _control.ModulesToProcess = Project.GetUnevaluatedProperty("ModuleToProcess");
            _control.NestedModules = Project.GetUnevaluatedProperty("NestedModules");
            _control.ScriptsToProcess = Project.GetUnevaluatedProperty("ScriptsToProcess");
            _control.TypesToProcess = Project.GetUnevaluatedProperty("TypesToProcess");

            _control.AliasesToExport = Project.GetUnevaluatedProperty("AliasesToExport");
            _control.CmdletsToExport = Project.GetUnevaluatedProperty("CmdletsToExport");
            _control.VariablesToExport = Project.GetUnevaluatedProperty("VariablesToExport");

            _control.Author = Project.GetUnevaluatedProperty("Author");
            _control.Company = Project.GetUnevaluatedProperty("CompanyName");
            _control.Copyright = Project.GetUnevaluatedProperty("Copyright");
            _control.Description = Project.GetUnevaluatedProperty("Description");
            _control.Version = Project.GetUnevaluatedProperty("Version");
            _control.Guid = Project.GetUnevaluatedProperty("Guid");

            if (string.IsNullOrEmpty(_control.Guid))
            {
                _control.Guid = Guid.NewGuid().ToString();
            }

            _control.ClrVersion = Project.GetUnevaluatedProperty("ClrVersion");
            _control.PowerShellHostVersion = Project.GetUnevaluatedProperty("PowerShellHostVersion");
            _control.PowerShellVersion = Project.GetUnevaluatedProperty("PowerShellVersion");
            _control.ProcessorArchitecture = Project.GetUnevaluatedProperty("ProcessorArchitecture");
            _control.RequiredAssemblies = Project.GetUnevaluatedProperty("RequiredAssemblies");
            _control.RequiredModules = Project.GetUnevaluatedProperty("RequiredModules");

            //bool generateManifest;
            //var manifest = Project.GetUnevaluatedProperty("GenerateModuleManifest");
            //if (bool.TryParse(manifest, out generateManifest))
            //{
            //    _control.GenerateModuleManifest = generateManifest;
            //}

            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Module Manifest"; }
        }
    }
}
