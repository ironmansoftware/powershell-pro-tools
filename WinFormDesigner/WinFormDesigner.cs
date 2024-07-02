using System;
using System.Windows.Forms;
using IM.WinForms;
using IMS.FormDesigner.Languages;
using PowerShellToolsPro.FormsDesigner;

namespace WinFormDesigner
{
    public partial class frmWinFormDesigner : Form
    {
        private frmMain mainForm;
        private bool dirty = false;

        public frmWinFormDesigner()
        {
            InitializeComponent();
        }


        public frmWinFormDesigner(string codeFile, string designerFile)
        {
            mainForm = new frmMain(new PowerShellLanguage(), designerFile, codeFile, EventGenerationType.Variable);
            mainForm.DesignerDirty += MainFormOnDesignerDirty;

            InitializeComponent();

            btnSave.Click += (sender, args) =>
            {
                if (!dirty) return;

                mainForm.Save();
                dirty = false;
                Text = "Windows Form Designer";
                btnSave.Enabled = false;
            };

        }

        private void MainFormOnDesignerDirty(object sender, EventArgs e)
        {
            Text = "Windows Form Designer *";
            dirty = true;
            btnSave.Enabled = true;
        }

        private void frmWinFormDesigner_Load(object sender, EventArgs e)
        {
            splitContainer1.Panel2.Controls.Add(mainForm);
            mainForm.Dock = DockStyle.Fill;

            tabProperties.Controls.Add(mainForm.propertyGrid);
            mainForm.propertyGrid.Dock = DockStyle.Fill;

            tabToolbox.Controls.Add(mainForm.lstToolbox);
            mainForm.lstToolbox.Dock = DockStyle.Fill;
        }
    }
}
