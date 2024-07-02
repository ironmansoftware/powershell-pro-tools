using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Remove, "VSCodeTextEditor")]
    public class RemoveTextEditorCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public VsCodeTextEditor TextEditor { get; set; }

        protected override void ProcessRecord()
        {
            SendCommand($"vscode.TextEditor.hide", new { filePath = TextEditor.Document.FileName });
        }
    }
}
