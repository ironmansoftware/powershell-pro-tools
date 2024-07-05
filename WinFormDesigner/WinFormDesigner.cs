using System;
using System.IO;
using System.Windows.Forms;
using IM.WinForms;
using IMS.FormDesigner.Languages;
using PowerShellToolsPro.FormsDesigner;

namespace WinFormDesigner
{
    public partial class frmWinFormDesigner : Form
    {
        private frmMain mainForm;
        private bool dirty;

        public frmWinFormDesigner()
        {
            InitializeComponent();
        }


        public frmWinFormDesigner(string codeFile, string designerFile)
        {
            if (!File.Exists(codeFile))
            {
                MessageBox.Show("Code file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (dirty && keyData == (Keys.Control | Keys.S))
            {
                mainForm.Save();
                dirty = false;
                Text = "Windows Form Designer";
                btnSave.Enabled = false;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
