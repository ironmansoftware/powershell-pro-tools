using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommunications.Send, "VSCodeTerminalText")]
    public class SendTerminalTextCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "name", ValueFromPipeline = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "terminal", ValueFromPipeline = true)]
        public VsCodeTerminal Terminal { get; set; }

        [Parameter(Mandatory = true)]
        public string Text { get; set;}

        [Parameter()]
        public SwitchParameter AddNewLine { get; set; }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "terminal")
            {
                Name = Terminal.Name;
            }

            SendCommand($"vscode.Terminal.sendText", new { text = Text, addNewLine = AddNewLine.IsPresent, name = Name });
        }
    }
}
