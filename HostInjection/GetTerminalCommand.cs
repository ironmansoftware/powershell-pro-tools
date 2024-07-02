using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Get, "VSCodeTerminal")]
    public class GetTerminalCommand : VSCodeCmdlet
    {
        protected override void BeginProcessing()
        {
            Wait = SwitchParameter.Present;

            var result = SendCommand($"vscode.window.terminals", new {});

            if (result != null)
            {
                var obj = JsonConvert.DeserializeObject<VsCodeTerminal[]>(result);
                WriteObject(obj, true);
            }
        }
    }
}
