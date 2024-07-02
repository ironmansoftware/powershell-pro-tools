using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using PowerShellToolsPro;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class DebugPropertyPageControl : UserControl
    {
        private CommonPropertyPage _page;

        public bool LoadingSettings { get; set; }

        public DebugPropertyPageControl(CommonPropertyPage page)
        {
            _page = page;
            InitializeComponent();

            cmoScript.SelectedIndexChanged += CmoScript_SelectedIndexChanged;
            txtArguments.TextChanged += txtArguments_TextChanged;
            Load += DebugPropertyPageControl_Load;
        }

        private void CmoScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!LoadingSettings)
                _page.IsDirty = true;
        }

        private void DebugPropertyPageControl_Load(object sender, EventArgs e)
        {
            LoadingSettings = true;
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var visualStudio = componentModel.DefaultExportProvider.GetExports<IVisualStudio>().First().Value;

            cmoScript.Items.Add(ProjectConstants.CurrentlyOpenScript);

            foreach (var file in visualStudio.ActiveWindowProject.Files)
            {
                string fileName = file.FileName;
                if (file.FileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) && File.Exists(file.FileName + ".ps1"))
                {
                    fileName = file.FileName + ".ps1";
                }

                if (!fileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)) continue;
                var fileInfo = new FileInfo(visualStudio.ActiveWindowProject.FullName);

                fileName = fileName.Replace(fileInfo.DirectoryName, string.Empty);
                fileName = fileName.TrimStart('\\');

                cmoScript.Items.Add(fileName);
            }

            LoadingSettings = false;
        }

        void txtArguments_TextChanged(object sender, EventArgs e)
        {
            if (!LoadingSettings)
                _page.IsDirty = true;
        }

        public string Arguments
        {
            get { return txtArguments.Text; }
            set { txtArguments.Text = value; }
        }

        public string Script
        {
            get { return cmoScript.SelectedText; }
            set { cmoScript.SelectedText = value; }
        }
    }
}
