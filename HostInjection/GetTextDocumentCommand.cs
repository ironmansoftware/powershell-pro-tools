using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Get, "VSCodeTextDocument")]
    public class GetTextDocumentCommand : VSCodeCmdlet
    {
        protected override void BeginProcessing()
        {
            Wait = SwitchParameter.Present;
            
            var result = SendCommand($"vscode.workspace.textDocuments", new {});

            if (result != null)
            {
                var obj = JsonConvert.DeserializeObject<VsCodeTextDocument[]>(result);
                WriteObject(obj, true);
            }
        }
    }
}
