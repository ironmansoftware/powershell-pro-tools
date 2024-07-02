using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Show, "VSCodeMessage")]
    public class ShowMessageCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Message { get; set; }

        [Parameter()]
        public string[] Items { get; set; }

        [Parameter()]
        public SwitchParameter Modal { get; set; }

        [Parameter()]
        [ValidateSet("Error", "Warning", "Information")]
        public string Type { get; set; } = "Information";

        protected override void BeginProcessing()
        {
            if (Items != null) {
                Wait = SwitchParameter.Present;
            }

            var result = SendCommand($"vscode.window.show{Type}Message", new { message = Message, options = new { modal = Modal.IsPresent },   items = Items });

            if (result != null)
                WriteObject(result);
        }
    }
}
