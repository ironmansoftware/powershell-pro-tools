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
using System.Security.Cryptography;

#pragma warning disable SYSLIB0021, SYSLIB0012

namespace PowerShellToolsPro.Packager.ConsoleHost
{
    class Program
    {
        [DllImport("Kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("Kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        static int Main(string[] args)
        {
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

            AllocConsole();
            HideConsoleWindow(HideConsoleWindowSwitch);

            //QuickEdit

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
                var consoleHostType = typeof(ConsoleShell).Assembly.GetType("Microsoft.PowerShell.ConsoleHost");
                var prop = consoleHostType.GetField("DefaultInitialSessionState", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                prop.SetValue(null, InitialSessionState.CreateDefault2());

                var myArgs = new List<string>();
                //Arguments
                myArgs.AddRange(new[] { "-ExecutionPolicy", "Unrestricted" }); //, "-Command", $"& {{ {contents.TrimEnd('\r', '\n')} }}" });
                myArgs.AddRange(new[] { "-Command", contents.TrimEnd('\r', '\n') });
                myArgs.AddRange(arguments);
                myArgs.AddRange(new[] { "-PoshToolsRoot", "\"" + AssemblyDirectory + "\"" });
                return ConsoleShell.Start(null, null, myArgs.ToArray());
            }
            finally
            {
                DeleteModuleDirectory(modulePath);
            }
        }

        public static void HideConsoleWindow(bool hideConsole)
        {
            if (!hideConsole) return;

            IntPtr hConsole = GetConsoleWindow();
            if (IntPtr.Zero != hConsole)
            {
                ShowWindow(hConsole, 0);
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
            Environment.SetEnvironmentVariable("PSModulePath", path);
        }

        private static void DeleteModuleDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    var powershell = new Process();
                    powershell.StartInfo = new ProcessStartInfo();
                    powershell.StartInfo.CreateNoWindow = true;
                    powershell.StartInfo.FileName = "powershell";
                    powershell.StartInfo.Arguments = $"-NoProfile -NonInteractive -Command \"Start-Sleep 2; Remove-Item '{directory}' -Force -Recurse\"";
                    powershell.Start();
                }
            }
            catch { }

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

        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

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