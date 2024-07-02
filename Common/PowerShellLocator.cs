using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerShellTools.Options
{
    public class PowerShellLocator
    {
        public PowerShellLocator()
        {
            PowerShellVersions = new Dictionary<string, string>();
            GetPowerShellOptions();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        public Dictionary<string, string> PowerShellVersions { get; private set; }

        public string DefaultVersion
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    return "Windows PowerShell (x64)";
                }
                else
                {
                    return "Windows PowerShell (x86)";
                }
            }
        }

        private void GetPowerShellOptions()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                PowerShellVersions.Add("Windows PowerShell (x64)", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"));
                PowerShellVersions.Add("Windows PowerShell (x86)", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "WindowsPowerShell", "v1.0", "powershell.exe"));
                var size = IntPtr.Size == 4 ? "x86" : "x64";
                PowerShellVersions.Add($"Windows PowerShell (Integrated - {size})", string.Empty);
            }
            else
            {
                PowerShellVersions.Add("Windows PowerShell (x86)", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"));
                PowerShellVersions.Add($"Windows PowerShell (Integrated - x86)", string.Empty);
            }

            FindPSCoreWindowsInstallation();
            FindPSCoreWindowsInstallation(true);
            FindPSCoreWindowsInstallation(true, true);
            FindPSCoreWindowsInstallation(false, true);
        }

        // This is required, since parseInt("7-preview") will return 7.
        private static Regex IntRegex = new Regex("/^\\d+$/;");

        private void FindPSCoreWindowsInstallation(bool useAlternateBitness = false, bool findPreview = false)
        {
            IntPtr wow64Value = IntPtr.Zero;
            Wow64DisableWow64FsRedirection(ref wow64Value);

            var programFilesPath = useAlternateBitness ? Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%") : Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var powerShellInstallBaseDir = Path.Combine(programFilesPath, "PowerShell");

            if (!Directory.Exists(powerShellInstallBaseDir))
            {
                return;
            }

            var highestSeenVersion = -1;
            string pwshExePath = null;
            foreach (var dir in Directory.GetDirectories(powerShellInstallBaseDir)) {
                var directoryInfo = new DirectoryInfo(dir);
                var item = directoryInfo.Name;

                var currentVersion = -1;
                if (findPreview) {
                    // We are looking for something like "7-preview"

                    // Preview dirs all have dashes in them
                    var dashIndex = item.IndexOf("-");
                    if (dashIndex < 0) {
                        continue;
                    }

                    // Verify that the part before the dash is an integer
                    var intPart = item.Substring(0, dashIndex);
                    if (!int.TryParse(intPart, out int value)) {
                        continue;
                    }

                    // Verify that the part after the dash is "preview"
                    if (item.Substring(dashIndex + 1) != "preview") {
                        continue;
                    }

                    currentVersion = int.Parse(intPart);
                } else {
                    // Search for a directory like "6" or "7"
                    if (!int.TryParse(item, out int value)) {
                        continue;
                    }

                    currentVersion = int.Parse(item);
                }

                // Ensure we haven't already seen a higher version
                if (currentVersion <= highestSeenVersion) {
                    continue;
                }

                // Now look for the file
                var exePath = Path.Combine(powerShellInstallBaseDir, item, "pwsh.exe");
                if (!File.Exists(exePath)) {
                    continue;
                }

                pwshExePath = exePath;
                highestSeenVersion = currentVersion;
            }

            if (string.IsNullOrEmpty(pwshExePath)) {
                return;
            }

            var bitness = programFilesPath.IndexOf("x86") != -1
                ? "(x86)"
                : "(x64)";

            var preview  = findPreview ? " Preview" : "";

            PowerShellVersions.Add($"PowerShell {highestSeenVersion}{preview} { bitness}", pwshExePath);
        }
    }
}
