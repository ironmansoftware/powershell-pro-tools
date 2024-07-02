using Microsoft.Win32;
using System;

namespace PowerShellTools.Common
{
    public static class DependencyUtilities
    {
        public static Version GetInstalledPowerShellVersion()
        {
            var version = new Version(0, 0);
            using (var reg = Registry.LocalMachine.OpenSubKey(Constants.NewInstalledPowerShellRegKey))
            {
                if (reg != null)
                {
                    var versionString = reg.GetValue(Constants.PowerShellVersionRegKeyName) as string;

                    Version.TryParse(versionString, out version);
                    return version;
                }
            }

            using (var reg = Registry.LocalMachine.OpenSubKey(Constants.LegacyInstalledPowershellRegKey))
            {
                if (reg != null)
                {
                    var versionString = reg.GetValue(Constants.PowerShellVersionRegKeyName) as string;
                    Version.TryParse(versionString, out version);
                    return version;
                }
            }

            return version;
        }
    }
}
