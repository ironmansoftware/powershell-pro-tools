using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Set, "VSCodeStatusBarMessage")]
    public class SetStatusBarMessageCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Message { get; set; }

        [Parameter()]
        public int Timeout { get; set; } = 5000;

        protected override void BeginProcessing()
        {
            var result = SendCommand($"vscode.window.setStatusBarMessage", new { message = Message, hideAfterTimeout = Timeout });

            if (result != null)
                WriteObject(result);
        }
    }
}
