using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Microsoft.PowerShell;
using System.Runtime.InteropServices;
using System;
using System.ComponentModel;
using System.Text;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography;

namespace PowerShellToolsPro.Packager.ConsoleHost
{
    class Program
    {
        private const string CleanupArgument = "--poshtools-cleanup";

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static Process _console;

        static int Main(string[] args)
        {
            if (args.Length == 2 && args[0] == CleanupArgument)
            {
                DeleteModuleDirectory(args[1]);
                return 0;
            }

            // License

            var arguments = new List<string>();
            foreach (var arg in args)
            {
                var argument = arg;
                if (arg.Contains(" "))
                {
                    argument = $"'{arg}'";
                }

                arguments.Add(argument);
            }

            AttachConsole();

            var highDpi = HighDpiSetting;
            if (highDpi)
            {
                SetProcessDPIAware();
            }

            var moduleZip = Path.GetTempFileName();
            var modulePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (WriteResourceToFile("//FileName.Modules.zip", moduleZip))
            {
                ZipFile.ExtractToDirectory(moduleZip, modulePath);
                AddValueToPathEnvVar(modulePath);
            }

            String script;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("//FileName.script.ps1"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    script = reader.ReadToEnd();
                }
            }

#if OBFUSCATE
            script = Decrypt(script);
#endif

            try
            {
                var contents = ReplaceString(script, "$PSScriptRoot", "$PoshToolsRoot", StringComparison.OrdinalIgnoreCase);

                var myArgs = new List<string>();
                //Arguments
                myArgs.AddRange(new[] { "-Command", contents.TrimEnd('\r', '\n') });
                myArgs.AddRange(arguments);
                myArgs.AddRange(new[] { "-PoshToolsRoot", "\"" + AssemblyDirectory + "\"" });

                return ConsoleShell.Start(RunspaceConfiguration.Create(), null, null, myArgs.ToArray());
            }
            finally
            {
                _console?.Kill();
                ScheduleModuleDirectoryCleanup(modulePath);
            }
        }

#if OBFUSCATE
        private static string Decrypt(string cipherString)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            string key = "//key";
            keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
#endif

        public static bool WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null) return false;

                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }

                return true;
            }
        }

        public static void AddValueToPathEnvVar(string path)
        {
            var pathvar = Environment.GetEnvironmentVariable("PSModulePath");
            pathvar += ";" + path;
            Environment.SetEnvironmentVariable("PSModulePath", pathvar);
        }

        private static void ScheduleModuleDirectoryCleanup(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            var cleanup = new Process();
            cleanup.StartInfo = new ProcessStartInfo();
            cleanup.StartInfo.UseShellExecute = false;
            cleanup.StartInfo.CreateNoWindow = true;
            cleanup.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            cleanup.StartInfo.Arguments = CleanupArgument + " " + QuoteArgument(directory);
            cleanup.Start();
        }

        private static void DeleteModuleDirectory(string directory)
        {
            for (var retry = 0; retry < 30; retry++)
            {
                if (!Directory.Exists(directory))
                {
                    return;
                }

                try
                {
                    Directory.Delete(directory, true);
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static string QuoteArgument(string argument)
        {
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
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
    }
}