using System;
using System.Diagnostics;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsCommon.Show, "PSScriptPad")]
    public class ShowPSScriptPadCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string Path { get; set; }

        protected override void BeginProcessing()
        {
            var windows = SessionState.PSVariable.GetValue("IsWindows");
            if (windows != null && !(bool)windows)
            {
                throw new Exception("This cmdlet is only supported on Windows");
            }

            var assemblyBasePath = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            var designer = System.IO.Path.Combine(assemblyBasePath, "FormDesigner", "PSScriptPad.exe");

            var args = string.Empty;
            if (!string.IsNullOrEmpty(Path))
            {
                var path = base.GetUnresolvedProviderPathFromPSPath(Path);
                args = $"-c=\"{path}\"";
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = designer;
            process.StartInfo.Arguments = args;
            process.Start();
        }
    }
}
