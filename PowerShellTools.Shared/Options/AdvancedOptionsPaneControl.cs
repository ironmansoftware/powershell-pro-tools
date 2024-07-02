using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PowerShellToolsPro.Options
{
    public partial class AdvancedOptionsPaneControl : UserControl
    {
        private bool _loading;
        private readonly AdvancedOptionsPane _pane;
        public AdvancedOptionsPaneControl(AdvancedOptionsPane pane)
        {
            _pane = pane;
            this.Dock = System.Windows.Forms.DockStyle.Fill;

            InitializeComponent();

            chkPackage.CheckedChanged += (sender, args) => pane.RaiseOnDirty();

            cmoPlatform.SelectedIndex = 0;

            cmoPowerShellVersion.Items.AddRange(new object[] {
            "Windows PowerShell",
            "7.0.0",
            "7.0.1",
            "7.0.2",
            "7.0.3",
            "7.0.6",
            "7.1.0",
            "7.1.1",
            "7.1.2",
            "7.1.3",
            "7.1.4",
            "7.1.5",
            "7.2.0",
            "7.2.1",
            "7.2.2",
            "7.2.3",
            "7.2.4",
            "7.2.5",
            "7.2.6"});
        }

        public string PowerShellVersion
        {
            get { return cmoPowerShellVersion.SelectedItem as string;  }
            set { cmoPowerShellVersion.SelectedItem = value; }
        }

        public bool PackageAsExecutable
        {
            get { return chkPackage.Checked; }
            set { chkPackage.Checked = value;  }
        }

        public bool Bundle
        {
            get { return chkBundle.Checked; }
            set { chkBundle.Checked = value; }
        }

        public string PackageEntryPoint
        {
            get;
            set;
        }

        public bool Obfuscate
        {
            get { return chkObfuscate.Checked; }
            set { chkObfuscate.Checked = value; }
        }

	    public bool HideConsoleWindow
	    {
		    get { return chkHideConsoleWindow.Checked; }
		    set { chkHideConsoleWindow.Checked = value; }
	    }

        public string DotNetVersion
        {
            get { return cmoDotNetVersion.SelectedItem as string; }
            set { cmoDotNetVersion.SelectedItem = value; }
        }

        public string FileVersion
        {
            get { return txtFileVersion.Text;  }
            set { txtFileVersion.Text = value; }
        }

        public string FileDescription
        {
            get { return txtFileDescription.Text; }
            set { txtFileDescription.Text = value; }
        }

        public new string ProductName
        {
            get { return txtProductName.Text; }
            set { txtProductName.Text = value; }
        }

        public new string ProductVersion
        {
            get { return txtProductVersion.Text; }
            set { txtProductVersion.Text = value; }
        }

        public string Copyright
        {
            get { return txtCopyright.Text; }
            set { txtCopyright.Text = value; }
        }

        public bool RequireElevation
        {
            get { return chkRequireElevation.Checked; }
            set { chkRequireElevation.Checked = value; }
        }

        public bool PackageModules
        {
            get { return chkPackageModules.Checked; }
            set { chkPackageModules.Checked = value; }
        }

        public string ApplicationIconPath
        {
            get { return txtIcon.Text; }
            set { txtIcon.Text = value; }
        }

        public string PackageType
        {
            get { return cmoPackageType.SelectedItem as string; }
            set { cmoPackageType.SelectedItem = value; }
        }

        public string ServiceName
        {
            get { return txtServiceName.Text;  }
            set { txtServiceName.Text = value; }
        }

        public string ServiceDisplayName
        {
            get { return txtServiceDisplayName.Text; }
            set { txtServiceDisplayName.Text = value; }
        }

        public bool HighDPISupport
        {
            get { return chkHighDpiSupport.Checked; }
            set { chkHighDpiSupport.Checked = value; }
        }

        public string Platform
        {
            get { return cmoPlatform.SelectedItem as string; }
            set { cmoPlatform.SelectedItem = value; }
        }


        public string PowerShellArgs
        {
            get { return txtPowerShellArgs.Text; }
            set { txtPowerShellArgs.Text = value; }
        }

        private void AdvancedOptionsPaneControl_Load(object sender, EventArgs e)
        {
            _loading = true;
            foreach (var file in _pane.VisualStudio.ActiveWindowProject.Files)
            {
                string fileName = file.FileName;
                if (file.FileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) && File.Exists(file.FileName + ".ps1"))
                {
                    fileName = file.FileName + ".ps1";
                }

                if (!fileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)) continue;
                var fileInfo = new FileInfo(_pane.VisualStudio.ActiveWindowProject.FullName);

                fileName = fileName.Replace(fileInfo.DirectoryName, string.Empty);
                fileName = fileName.TrimStart('\\');

                cmoEntryScript.Items.Add(fileName);
            }

            if (string.IsNullOrEmpty(PackageEntryPoint))
            {
                cmoEntryScript.SelectedItem = cmoEntryScript.Items.Count > 0 ? cmoEntryScript.Items[0] : null;
				if (cmoEntryScript.SelectedItem != null)
					_pane.PackageEntryPoint = cmoEntryScript.SelectedItem.ToString();	
			}
            else
            {
                cmoEntryScript.SelectedItem = PackageEntryPoint;
            }

            if (string.IsNullOrEmpty(PowerShellVersion))
            {
                cmoPowerShellVersion.SelectedItem = "Windows PowerShell";
                if (cmoPowerShellVersion.SelectedItem != null)
                    _pane.PowerShellVersion = cmoPowerShellVersion.SelectedItem.ToString();
            }

            if (string.IsNullOrEmpty(DotNetVersion))
            {
                cmoDotNetVersion.SelectedItem = "net462";
                if (cmoDotNetVersion.SelectedItem != null)
                    _pane.DotNetVersion = cmoDotNetVersion.SelectedItem.ToString();
            }
            else
            {
                cmoDotNetVersion.SelectedItem = DotNetVersion;
            }

            if (string.IsNullOrEmpty(PackageType))
            {
                PackageType = "Console";
            }

            _loading = false;
        }

        private void chkPackage_CheckedChanged(object sender, EventArgs e)
        {
            _pane.PackageAsExecutable = chkPackage.Checked;
        }

        private void cmoEntryScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            _pane.PackageEntryPoint = cmoEntryScript.SelectedItem.ToString();
        }

        private void chkBundle_CheckedChanged(object sender, EventArgs e)
        {
            _pane.Bundle = Bundle;
        }

        private void chkObfuscate_CheckedChanged(object sender, EventArgs e)
        {
            _pane.Obfuscate = Obfuscate;
        }

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			_pane.HideConsoleWindow = HideConsoleWindow;
		}

        private void cmoDotNetVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pane.DotNetVersion = DotNetVersion;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _pane.FileVersion = FileVersion;
        }

        private void chkRequireElevation_CheckedChanged(object sender, EventArgs e)
        {
            _pane.RequireElevation = RequireElevation;
        }

        private void chkPackageModules_CheckedChanged(object sender, EventArgs e)
        {
            _pane.PackageModules = PackageModules;
        }

        private void txtFileDescription_TextChanged(object sender, EventArgs e)
        {
            _pane.FileDescription = FileDescription;
        }

        private void txtProductName_TextChanged(object sender, EventArgs e)
        {
            _pane.ProductName = ProductName;
        }

        private void txtProductVersion_TextChanged(object sender, EventArgs e)
        {
            _pane.ProductVersion = ProductVersion;
        }

        private void txtCopyright_TextChanged(object sender, EventArgs e)
        {
            _pane.Copyright = Copyright;
        }

        private void btnExploreIcon_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtIcon.Text = dialog.FileName;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void txtIcon_TextChanged(object sender, EventArgs e)
        {
            _pane.ApplicationIconPath = ApplicationIconPath;
        }

        private void cmoPackageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmoPackageType.SelectedItem.ToString().Equals("console", StringComparison.OrdinalIgnoreCase))
            {
                txtServiceDisplayName.Enabled = false;
                txtServiceName.Enabled = false;
            }
            else
            {
                txtServiceDisplayName.Enabled = true;
                txtServiceName.Enabled = true;
            }

            _pane.PackageType = cmoPackageType.SelectedItem.ToString();
        }

        private void txtServiceName_TextChanged(object sender, EventArgs e)
        {
            _pane.ServiceName = ServiceName;
        }

        private void txtServiceDisplayName_TextChanged(object sender, EventArgs e)
        {
            _pane.ServiceDisplayName = ServiceDisplayName;
        }

        private void ChkHighDpiSupport_CheckedChanged(object sender, EventArgs e)
        {
            _pane.HighDPISupport = HighDPISupport;
        }

        private void TxtPowerShellArgs_TextChanged(object sender, EventArgs e)
        {
            _pane.PowerShellArgs = PowerShellArgs;
        }

        private void chkPackagePowerShell_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cmoPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pane.PackagePlatform = Platform;
        }

        private void cmoPowerShellVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pane.PowerShellVersion = PowerShellVersion;

            if (Version.TryParse(PowerShellVersion, out Version version) && version.Major == 7 && version.Minor == 0)
            {
                cmoDotNetVersion.SelectedItem = "netcoreapp31";
                cmoDotNetVersion.Enabled = false;
            }
            else if (Version.TryParse(PowerShellVersion, out Version version2) && version2.Major == 7 && version2.Minor == 1)
            {
                cmoDotNetVersion.SelectedItem = "net5.0";
                cmoDotNetVersion.Enabled = false;
            }
            else if (Version.TryParse(PowerShellVersion, out Version version3) && version2.Major == 7 && version2.Minor == 2)
            {
                cmoDotNetVersion.SelectedItem = "net6.0";
                cmoDotNetVersion.Enabled = false;
            }
            else
            {
                cmoDotNetVersion.Enabled = true;
            }
        }
    }
}
