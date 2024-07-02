using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Add, "VSCodeTextDocumentText")]
    public class AddTextDocumentTextCommand : VSCodeCmdlet
    {

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public VsCodeTextDocument TextDocument { get; set; }

        [Parameter(Mandatory = true)]
        public VsCodePosition Position { get; set; }

        [Parameter(Mandatory = true)]
        public string Text { get; set; }

        protected override void ProcessRecord()
        {
            Wait = SwitchParameter.Present;
            
            var result = SendCommand($"vscode.TextDocument.insert", new { fileName = TextDocument.FileName, position = Position, text = Text });
            WriteObject(result);
        }
    }
}
