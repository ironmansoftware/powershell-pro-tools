using System;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using IronmanPowerShellHost.Executors;
using System.Threading;
using PowerShellToolsPro.Packager.Config;

namespace IronmanPowerShellHost
{
    internal class Program
    {
        private static Process _console;
        static int Main(string[] args)
        {
            var resources = ResourceLoader.LoadResources();
            var config = resources.Config;
            var modules = resources.Modules;
            var script = resources.Script;

            if (config.DisableQuickEdit)
            {
                DisableQuickEdit();
            }

            if (config.HideConsoleWindow || config.Host == PowerShellHosts.IronmanPowerShellWinFormsHost)
            {
                AttachConsole();
            }

            var moduleZip = Path.GetTempFileName();
            var modulePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (modules != null)
            {
                if (WriteResourceToFile(new MemoryStream(modules), moduleZip))
                {
                    ZipFile.ExtractToDirectory(moduleZip, modulePath);
                    AddValueToPathEnvVar(modulePath);
                }
            }

            try
            {
                script = ReplaceString(script, "$PSScriptRoot", "$ExecutableRoot", StringComparison.OrdinalIgnoreCase);

                return ExecutorFactory.GetExecutor(config).Run(script, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing PowerShell: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
            finally
            {
                _console?.Kill();
                if (!string.IsNullOrEmpty(modulePath))
                    DeleteModuleDirectory(modulePath);
            }
        }

        public static bool WriteResourceToFile(Stream stream, string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(file);
            }

            return true;
        }

        public static void AddValueToPathEnvVar(string path)
        {
            var pathvar = Environment.GetEnvironmentVariable("PSModulePath");
            pathvar += ";" + path;
            Environment.SetEnvironmentVariable("PSModulePath", pathvar, EnvironmentVariableTarget.Process);
        }

        private static void DeleteModuleDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                var powershell = new Process();
                powershell.StartInfo = new ProcessStartInfo();
                powershell.StartInfo.UseShellExecute = false;
                powershell.StartInfo.CreateNoWindow = true;
                powershell.StartInfo.FileName = "powershell";
                powershell.StartInfo.Arguments = $"-WindowStyle Hidden -NoProfile -NonInteractive -Command \"Start-Sleep 2; Remove-Item '{directory}' -Force -Recurse\"";
                powershell.Start();
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static void DisableQuickEdit()
        {
            SetQuickEdit(false);
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);


        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);


        private static void AttachConsole()
        {
            if (!AttachConsole(-1))
            {
                _console = new Process();
                _console.StartInfo = new ProcessStartInfo();
                _console.StartInfo.UseShellExecute = false;
                _console.StartInfo.CreateNoWindow = true;
                _console.StartInfo.FileName = "cmd";
                _console.Start();

                var attached = AttachConsole(_console.Id);

                var tries = 0;
                while (tries < 100 && !attached)
                {
                    Thread.Sleep(100);
                    attached = AttachConsole(_console.Id);
                }
            }
        }

        public static bool SetQuickEdit(bool SetEnabled)
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            if (SetEnabled)
            {
                consoleMode &= ~ENABLE_QUICK_EDIT;
            }
            else
            {
                consoleMode |= ENABLE_QUICK_EDIT;
            }

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }
    }
}