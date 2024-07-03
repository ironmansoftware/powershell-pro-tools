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

        protected override void EndProcessing()
        {
            var windows = SessionState.PSVariable.GetValue("IsWindows");
            if (windows != null && !(bool)windows)
            {
                throw new Exception("This cmdlet is only supported on Windows");
            }

            var assemblyBasePath = Path.GetDirectoryName(GetType().Assembly.Location);
            var designer = Path.Combine(assemblyBasePath, "FormDesigner", "WinFormDesigner.exe");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = designer;
            process.StartInfo.Arguments = $"-c \"{CodeFilePath}\" -d \"{DesignerFilePath}\"";
            process.Start();
        }
    }
}
