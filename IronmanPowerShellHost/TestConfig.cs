using PowerShellToolsPro.Packager.Config;

namespace IronmanPowerShellHost
{
    internal class TestConfig
    {
        public static string GetScript()
        {
            return @"& { Get-ChildItem }";
        }

        public static PackageConfig GetPackageConfig()
        {
            return new PackageConfig
            {
                Host = PowerShellHosts.IronmanPowerShellHost
            };
        }
    }
}
