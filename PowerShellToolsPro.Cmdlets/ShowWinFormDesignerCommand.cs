using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsCommon.Show, "WinFormDesigner")]
    public class ShowWinFormDesignerCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string DesignerFilePath { get; set; }

        [Parameter(Mandatory = true)]
        public string CodeFilePath { get; set; }

        [Parameter()]
        public string Key { get; set; }
        
        protected override void EndProcessing()
        {
            var windows = SessionState.PSVariable.GetValue("IsWindows");
            if (windows != null && !(bool)windows)
            {
                throw new Exception("This cmdlet is only supported on Windows");
            }

            var assemblyBasePath = Path.GetDirectoryName(GetType().Assembly.Location);
            var designer = Path.Combine(assemblyBasePath, "FormDesigner", "PSScriptPad.exe");

            var keyArg = string.Empty;
            if (!string.IsNullOrEmpty(Key))
            {
                keyArg = $"-key=\"{Key}\"";
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = designer;
            process.StartInfo.Arguments = $"-c=\"{CodeFilePath}\" -d=\"{DesignerFilePath}\" {keyArg}";
            process.Start();
        }
    }
}
