using System;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
    public class ValidateStageTests
    {
        [Fact]
        public void ShouldRejectDotNetFrameworkWithPowerShellCore()
        {
            var process = new PackageProcess
            {
                Config = new PsPackConfig
                {
                    Package = new PackageConfig
                    {
                        DotNetVersion = "v4.6.2",
                        PowerShellVersion = "7.4.1"
                    }
                }
            };

            var ex = Assert.Throws<Exception>(() => new ValidateStage().Execute(process, null));

            Assert.Contains(".NET Framework target 'v4.6.2' is not supported with PowerShell '7.4.1'", ex.Message);
        }
    }
}
