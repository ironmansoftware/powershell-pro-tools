using PowerShellProTools.Host;
using PowerShellToolsPro;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PowerShellProTools.Common
{
    [Cmdlet("Start", "PoshToolsServer")]
    public class StartPoshToolsServerCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string PipeName { get; set; }

        protected override void EndProcessing()
        {
            var server = new PoshToolsServer();

            _ = Task.Run(() =>
            {
                server.Start(PipeName);
            });
        }
    }
}
