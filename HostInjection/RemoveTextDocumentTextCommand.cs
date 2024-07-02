using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Remove, "VSCodeTextDocumentText")]
    public class RemoveTextDocumentTextCommand : VSCodeCmdlet
    {

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public VsCodeTextDocument TextDocument { get; set; }

        [Parameter(Mandatory = true)]
        public VsCodeRange Range { get; set; }

        protected override void ProcessRecord()
        {
            Wait = SwitchParameter.Present;
            
            var result = SendCommand($"vscode.TextDocument.delete", new { fileName = TextDocument.FileName, range = Range });
            WriteObject(result);
        }
    }
}
