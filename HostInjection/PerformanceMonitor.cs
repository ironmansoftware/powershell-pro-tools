using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;

namespace PowerShellToolsPro
{
    public class PerformanceMonitor 
    {
        static bool stop;

        public static void Start()
        {
            stop = false;
            var thread = new Thread(PerformanceMonitorLoop);
            thread.Start();
        }

        public static void Stop()
        {
            stop = true;
        }

        private static void PerformanceMonitorLoop()
        {
            while(!stop)
            {
                Thread.Sleep(1000);

                var process = Process.GetCurrentProcess();

                try 
                {
                    using (var namedPipeClient = new NamedPipeClientStream(".", $"PPTPipePerformance{process.Id}", PipeDirection.InOut))
                    {
                        namedPipeClient.Connect();
                        var streamWriter = new StreamWriter(namedPipeClient);
                        var streamReader = new StreamReader(namedPipeClient);
                        
                        streamWriter.Write(JsonConvert.SerializeObject(new { cpu = process.TotalProcessorTime.TotalSeconds, memory = process.WorkingSet64 }));
                        streamWriter.Flush();
                    }
                } catch {}
            }
        }
    }
}