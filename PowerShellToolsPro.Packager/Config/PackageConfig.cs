namespace PowerShellToolsPro.Packager.Config
{
    public class PsPackConfig
    {
        public PsPackConfig()
        {
            Package = new PackageConfig();
            Bundle = new BundlerConfig();
        }
        public string Root { get; set; }
        public string OutputPath { get; set; }
        public PackageConfig Package { get; set; }
        public BundlerConfig Bundle { get; set; }
    }

    public class PackageConfig
    {
        public bool Enabled { get; set; }
        public bool Obfuscate { get; set; }
        public bool HideConsoleWindow { get; set; }
        public string DotNetVersion { get; set; }
        public string FileVersion { get; set; }
        public string FileDescription { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string CompanyName { get; set; }
        public string Copyright { get; set; }
        public bool RequireElevation { get; set; }
        public string ApplicationIconPath { get; set; }
        public PackageType PackageType { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceName { get; set; }
        public bool HighDpiSupport { get; set; }
        public string PowerShellArguments { get; set; }
        public string Platform { get; set; } = "x64";
        public string RuntimeIdentifier { get; set; } = "win-x64";
        public string PowerShellVersion { get; set; } = "Windows PowerShell";
        public bool PowerShellCore => PowerShellVersion?.Equals("Windows PowerShell", System.StringComparison.OrdinalIgnoreCase) != true;
        public bool DisableQuickEdit { get; set; }
        public string[] Resources { get; set; } = new string[0];
        public string DotNetSdk { get; set; }
        public string Certificate { get; set; }
        public string OutputName { get; set; }
        public PowerShellHosts Host { get; set; }
        public bool Lightweight { get; set; }
    }

    public enum PowerShellHosts
    {
        Default,
        IronmanPowerShellHost,
        IronmanPowerShellWinFormsHost,
        PowerShell727,
        PowerShell727Lightweight,
        PowerShell727Winforms,
        PowerShell727WinformsLightweight,
    }

    public class BundlerConfig
    {
        public bool Enabled { get; set; }
        public bool Modules { get; set; }
        public bool NestedModules { get; set; }
        public bool RequiredAssemblies { get; set; }
        public string[] IgnoredModules { get; set; }
    }

    public enum PackageType
    {
        Console,
        Service
    }
}
