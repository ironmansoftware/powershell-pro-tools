using PowerShellProTools.Host;
using System.Linq;
using System.Windows.Forms;

namespace PowerShellTools.Options
{
    internal partial class GeneralOptionsControl : UserControl
    {
        private readonly GeneralOptions _options;

        public GeneralOptionsControl(GeneralOptions options)
        {
            _options = options;
            this.Dock = DockStyle.Fill;

            InitializeComponent();

            var powershellLocator = new PowerShellLocator();

            this.cmoPowerShellVersion.Items.AddRange(powershellLocator.PowerShellVersions.Keys.ToArray());

            if (!string.IsNullOrEmpty(options.PowerShellVersion) && this.cmoPowerShellVersion.Items.Contains(options.PowerShellVersion))
            {
                this.cmoPowerShellVersion.SelectedItem = options.PowerShellVersion;
            }
            else
            {
                this.cmoPowerShellVersion.SelectedIndex = 0;
            }

            this.chkLoadProfiles.Checked = options.ShouldLoadProfiles;
            this.chkMultiLineRepl.Checked = options.MultilineRepl;
            this.chkOverrideExecutionPolicy.Checked = options.OverrideExecutionPolicyConfiguration;
            this.chkTabComplete.Checked = options.TabComplete;
            this.chkSta.Checked = options.Sta;
        }

        private void chkOverrideExecutionPolicy_CheckedChanged(object sender, System.EventArgs e)
        {
            _options.OverrideExecutionPolicyConfiguration = chkOverrideExecutionPolicy.Checked;
        }

        private void chkMultiLineRepl_CheckedChanged(object sender, System.EventArgs e)
        {
            _options.MultilineRepl = chkMultiLineRepl.Checked;
        }

        private void cmoPowerShellVersion_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (cmoPowerShellVersion.SelectedItem.ToString() == _options.PowerShellVersion) return;

            var value = cmoPowerShellVersion.SelectedItem.ToString();
            _options.PowerShellVersion = value;
        }

        private void chkLoadProfiles_CheckedChanged(object sender, System.EventArgs e)
        {
            _options.ShouldLoadProfiles = chkLoadProfiles.Checked;
        }

        private void chkTabComplete_CheckedChanged(object sender, System.EventArgs e)
        {
            _options.TabComplete = chkTabComplete.Checked;
        }

        private void chkSta_CheckedChanged(object sender, System.EventArgs e)
        {
            _options.Sta = chkSta.Checked;
        }
    }
}
