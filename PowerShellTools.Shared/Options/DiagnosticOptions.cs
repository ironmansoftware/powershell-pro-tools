using System.ComponentModel;

namespace PowerShellTools.Options
{
    internal class DiagnosticOptions : BaseOptionModel<DiagnosticOptions>
    {
        [DisplayName(@"Host Process Start Timeout")]
        [Description("The number of seconds to wait for the host process to start before timing out. ")]
        public int HostProcessTimeout { get; set; } = 5;

        [DisplayName(@"Display PowerShell Window")]
        [Description("If true, the PowerShell host window will be shown. This is useful in debugging issues with the host starting.")]
        public bool DisplayPowerShellWindow { get; set; }

        [DisplayName(@"Wait for Debugger")]
        [Description("If true, the PowerShell host will wait for the debugger to attached to it. ")]
        public bool WaitForDebugger { get; set; }
    }
}
