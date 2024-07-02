using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;

namespace IronmanPowerShellHost
{
    internal class ResourceSet
    {
        public string Script { get; set; }
        public PackageConfig Config { get; set; }
        public byte[] Modules { get; set;  }
    }
}
