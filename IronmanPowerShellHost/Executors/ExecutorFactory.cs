using PowerShellToolsPro.Packager.Config;
using System;
namespace IronmanPowerShellHost.Executors
{
    internal class ExecutorFactory
    {
        public static IExecutor GetExecutor(PackageConfig packageConfig)
        {
            switch (packageConfig.Host)
            {
                case PowerShellHosts.IronmanPowerShellHost:
                case PowerShellHosts.IronmanPowerShellWinFormsHost:
                    return new DefaultConsoleExecutor();
            }
            throw new NotImplementedException();
        }
    }
}
