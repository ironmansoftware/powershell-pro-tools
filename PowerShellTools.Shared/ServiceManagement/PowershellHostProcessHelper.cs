using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PowerShellTools.Common.Logging;
using PowerShellTools.Options;
using PowerShellProTools.Common;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Helper class for creating a process used to host the WCF service.
    /// </summary>
    internal static class PowershellHostProcessHelper
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;
        private const int SW_HIDE = 0;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        private static readonly ILog Log = LogManager.GetLogger(typeof(PowershellHostProcessHelper));

        public static PowerShellHostProcess CreatePowerShellHostProcess(string powershellVersion)
        {
            IntPtr wow64Value = IntPtr.Zero;
            Wow64DisableWow64FsRedirection(ref wow64Value);

            var powershellLocator = new PowerShellLocator();

            string powershellPath;
            if (powershellVersion.ToLower().Contains("Integrated"))
            {
                return null;
            }
            else if (powershellLocator.PowerShellVersions.ContainsKey(powershellVersion))
            {
                powershellPath = powershellLocator.PowerShellVersions[powershellVersion];
            }
            else
            {
                powershellPath = powershellLocator.PowerShellVersions[powershellLocator.DefaultVersion];
            }

            var windowsPs = powershellVersion.StartsWith("Windows PowerShell");

            if (string.IsNullOrEmpty(powershellPath))
            {
                return null;
            }

            Log.DebugFormat("Starting host path. Bitness: {0}", powershellPath);
            PowerShellToolsPackage.DebuggerReadyEvent.Reset();
            Process powerShellHostProcess = new Process();

            var sta = GeneralOptions.Instance.Sta && windowsPs ? "-Sta" : string.Empty;

            var hostServiceDll = typeof(ParentProcessWatcher).Assembly.Location;

            string powerShellArgs = $"-NoExit {sta} -NoProfile -NonInteractive -Command &{{ Import-Module '{hostServiceDll}'; [PowerShellProTools.Common.ParentProcessWatcher]::WatchProcess({Process.GetCurrentProcess().Id}); }}";

            Log.DebugFormat("Host path: '{0}' Host arguments: '{1}'", powershellPath, powerShellArgs);

            powerShellHostProcess.StartInfo.Arguments = powerShellArgs;
            powerShellHostProcess.StartInfo.FileName = powershellPath;

            if (DiagnosticOptions.Instance.DisplayPowerShellWindow)
            {
                powerShellHostProcess.StartInfo.CreateNoWindow = false;
                powerShellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                powerShellHostProcess.StartInfo.UseShellExecute = false;
                powerShellHostProcess.StartInfo.RedirectStandardInput = false;
                powerShellHostProcess.StartInfo.RedirectStandardOutput = false;
                powerShellHostProcess.StartInfo.RedirectStandardError = false;
            }
            else
            {
                powerShellHostProcess.StartInfo.CreateNoWindow = true;
                powerShellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                powerShellHostProcess.StartInfo.UseShellExecute = false;
                //powerShellHostProcess.StartInfo.RedirectStandardInput = true;
                powerShellHostProcess.StartInfo.RedirectStandardOutput = true;
                powerShellHostProcess.StartInfo.RedirectStandardError = true;

                powerShellHostProcess.OutputDataReceived += PowerShellHostProcess_OutputDataReceived;
                powerShellHostProcess.ErrorDataReceived += PowerShellHostProcess_ErrorDataReceived;
            }

            powerShellHostProcess.Start();

            if (!DiagnosticOptions.Instance.DisplayPowerShellWindow)
            {
                powerShellHostProcess.EnableRaisingEvents = true;

                powerShellHostProcess.BeginOutputReadLine();
                powerShellHostProcess.BeginErrorReadLine();
            }

            return new PowerShellHostProcess(powerShellHostProcess);
        }

        private static void PowerShellHostProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                PowerShellHostProcessOutput(e.Data);
            }
        }

        private static void PowerShellHostProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                PowerShellHostProcessOutput(e.Data);
            }
        }

        private static void PowerShellHostProcessOutput(string outputData)
        {
            Console.WriteLine(outputData);
            Log.Debug(outputData);
        }
    }

    /// <summary>
    /// The structure containing the process we want and a guid used for the WCF client to establish connection to the service.
    /// </summary>
    public class PowerShellHostProcess
    {
        public Process Process
        {
            get;
            private set;
        }

        public PowerShellHostProcess(Process process)
        {
            Process = process;
        }

        /// <summary>
        /// Write user input into standard input pipe(redirected)
        /// </summary>
        /// <param name="content">User input string</param>
        public void WriteHostProcessStandardInputStream(string content)
        {
            StreamWriter _inputStreamWriter = Process.StandardInput;

            // Feed into stdin stream
            _inputStreamWriter.WriteLine(content);
            _inputStreamWriter.Flush();
        }
    }
}
