using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Show, "VSCodeQuickPick")]
    public class ShowQuickPickCommand : VSCodeCmdlet
    {
        [Parameter()]
        public string PlaceHolder { get; set; }

        [Parameter(Mandatory = true)]
        public string[] Items { get; set; }

        [Parameter()]
        public SwitchParameter CanPickMany { get; set; }

        [Parameter()]
        public SwitchParameter IgnoreFocusOut { get; set; }

        protected override void BeginProcessing()
        {
            Wait = SwitchParameter.Present;
            var result = SendCommand($"vscode.window.showQuickPick", new { placeHolder = PlaceHolder, items = Items, canPickMany = CanPickMany.IsPresent, ignoreFocusOut = IgnoreFocusOut.IsPresent });

            if (result != null)
                WriteObject(result);
        }
    }
}
