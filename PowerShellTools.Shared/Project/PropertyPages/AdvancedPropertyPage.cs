using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Interfaces;
using PowerShellToolsPro.Options;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.AdvancedPropertiesPageGuid)]
    public class AdvancedPropertyPage : CommonPropertyPage
    {
        private readonly Control _advancedPropertyPage;
        private readonly IOptionsPane _pane;

        public AdvancedPropertyPage()
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var x = componentModel.DefaultExportProvider.GetExports<IOptionsPane>("AdvancedOptionsPane").FirstOrDefault();
            if (x == null)
            {
                _advancedPropertyPage = new AdvanvedPropertyPageControl(this);
            }
            else
            {
                _pane = x.Value;
                _advancedPropertyPage = _pane.Control;
                _pane.OnDirty += (sender, args) => IsDirty = true;
            }
        }

        public override Control Control
        {
            get { return _advancedPropertyPage; }
        }
        public override void Apply()
        {
            if (_pane == null) return;

            foreach (var item in _pane.Properties)
            {
                Project.SetProjectProperty(item.Key, item.Value);
            }

            IsDirty = false;
        }

        public override void LoadSettings()
        {
            if (_pane == null) return;

            Loading = true;

            var dictionary = new Dictionary<string, string>();

            foreach (var property in _pane.PropertyNames)
            {
                dictionary.Add(property, Project.GetUnevaluatedProperty(property));
            }
            Loading = false;

            _pane.Properties = dictionary;
        }

        public override string Name
        {
            get { return "Advanced"; }
        }
    }
}
