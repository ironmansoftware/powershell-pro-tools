using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Get, "VSCodeTextEditor")]
    public class GetTextEditorCommand : VSCodeCmdlet
    {
        protected override void BeginProcessing()
        {
            Wait = SwitchParameter.Present;
            
            var result = SendCommand($"vscode.window.visibleTextEditors", new {});

            if (result != null)
            {
                var obj = JsonConvert.DeserializeObject<VsCodeTextEditor[]>(result);
                WriteObject(obj, true);
            }
        }
    }
}
