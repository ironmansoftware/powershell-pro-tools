using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management.Automation;
using System.Text;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class VSCodeCmdlet : PSCmdlet
    {
        private static int processId;
        static VSCodeCmdlet() {
            processId = Process.GetCurrentProcess().Id;
        }

        [Parameter()]
        public SwitchParameter Wait { get; set; }

        [Parameter()]
        public int ResponseTimeout { get; set; } = 5000;

        internal static string SendCommand(string command, object argument, bool wait, int responseTimeout)
        {
            using (var namedPipeClient = new NamedPipeClientStream(".", $"PPTPipeCode{processId}", PipeDirection.InOut))
            {
                namedPipeClient.Connect();
                var streamWriter = new StreamWriter(namedPipeClient);
                var streamReader = new StreamReader(namedPipeClient);

                streamWriter.Write(JsonConvert.SerializeObject(new { type = command, args = argument }) + "!PS");
                streamWriter.Flush();

                if (wait)
                {
                    var task = streamReader.ReadToEndAsync();
                    task.Wait(responseTimeout);

                    if (task.IsCompleted)
                    {
                        return task.Result;
                    }

                    throw new System.Exception($"Did not receive response in {responseTimeout}ms");
                }
            }

            return null;
        }

        protected string SendCommand(string command, object argument)
        {
            return SendCommand(command, argument, Wait, ResponseTimeout);
        }
    }
}
