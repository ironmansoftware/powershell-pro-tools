using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Open, "VSCodeTextDocument")]
    public class OpenTextDocumentCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true)]
        public string FileName { get; set; }
        protected override void BeginProcessing()
        {
            var path = base.GetUnresolvedProviderPathFromPSPath(FileName);

            if (!File.Exists(path))
            {
                throw new System.Exception("File not found.");
            }

            var result = SendCommand($"vscode.window.openTextDocument", path);

            if (result != null)
            {
                var obj = JsonConvert.DeserializeObject<VsCodeTextDocument>(result);
                WriteObject(obj);
            }
        }
    }
}
