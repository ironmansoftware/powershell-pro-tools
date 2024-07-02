using System;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class GeneralPropertyPageControl : UserControl
    {
        private readonly CommonPropertyPage _page;
        
        public bool LoadingSettings { get; set; }

        public GeneralPropertyPageControl(CommonPropertyPage page)
        {
            InitializeComponent();
            _page = page;

            txtOutputDirectory.TextChanged += Changed;
        }

        void Changed(object sender, EventArgs e)
        {
            if (!LoadingSettings)
                _page.IsDirty = true;
        }

        public string OutputDirectory
        {
            get { return txtOutputDirectory.Text; }
            set { txtOutputDirectory.Text = value; }
        }

        private void btnOutputDirectory_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                fbd.Description = Resources.OutputDirectory_Description;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    OutputDirectory = fbd.SelectedPath;
                }
            }

        }
    }
}
