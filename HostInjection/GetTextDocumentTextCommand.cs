using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Get, "VSCodeTextDocumentText")]
    public class GetTextDocumentTextCommand : VSCodeCmdlet
    {

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public VsCodeTextDocument TextDocument { get; set; }

        [Parameter()]
        public VsCodeRange Range { get; set; }

        protected override void ProcessRecord()
        {
            Wait = SwitchParameter.Present;
            
            var result = SendCommand($"vscode.TextDocument.getText", new { fileName = TextDocument.FileName, range = Range });
            WriteObject(result);
        }
    }
}
