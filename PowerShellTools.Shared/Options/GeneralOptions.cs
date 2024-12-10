using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.Options
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        public GeneralOptions()
        {
            this.OverrideExecutionPolicyConfiguration = true;
            this.MultilineRepl = false;
            this.ShouldLoadProfiles = true;
            this.TabComplete = true;
            this.Sta = false;

            var powershellLocator = new PowerShellLocator();
            this.PowerShellVersion = powershellLocator.DefaultVersion;
        }

        public event EventHandler<EventArgs> PowerShellVersionChanged;

        public override void Load()
        {
            var previousVersion = PowerShellVersion;

            base.Load();

            if (previousVersion != PowerShellVersion)
            {
                OnPowerShellVersionChanged();
            }
        }

        public override async Task LoadAsync()
        {
            var previousVersion = PowerShellVersion;

            await base.LoadAsync();

            if (previousVersion != PowerShellVersion)
            {
                OnPowerShellVersionChanged();
            }
        }

        public void OnPowerShellVersionChanged()
        {
            PowerShellVersionChanged?.Invoke(this, new EventArgs());
        }

        [DisplayName(@"Enable Unrestricted Execution Policy")]
        [Description("This setting controls the execution policy for executing PowerShell scripts in Visual Studio. True, will set the Visual Studio process execution policy to \"Unrestricted\".  False, will use the current user or local machine policy.")]
        public bool OverrideExecutionPolicyConfiguration { get; set; }

        [DisplayName(@"Multiline REPL Window")]
        [Description("When false, pressing enter invokes the command line in the REPL Window rather than starting a new line.")]
        public bool MultilineRepl { get; set; }

        [DisplayName(@"Load Profiles on Start")]
        [Description("When false, the host service will not load any profiles on startup.")]
        public bool ShouldLoadProfiles { get; set; }

        [DisplayName(@"PowerShell Version")]
        [Description("The PowerShell Version to run.")]
        public string PowerShellVersion { get; set; }

        [DisplayName(@"Tab Complete")]
        [Description("When true, enable tab complete for PowerShell scripts.")]
        public bool TabComplete { get; set; }

        [DisplayName(@"Apartment State")]
        public bool Sta { get; set; }
    }
}
