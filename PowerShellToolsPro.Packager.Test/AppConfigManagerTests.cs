using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace PowerShellToolsPro.Packager.Test
{
    public class AppConfigManagerTests
    {
        [Fact]
        public void ShouldGenerateEmptyAppConfig()
        {
            var appConfigManager = new AppConfigManager();
            var actual = XDocument.Parse(appConfigManager.GenerateAppConfig(new Config.PackageConfig()));
            var expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration/>");

            Assert.True(XNode.DeepEquals(actual, expected));
        }

        [Fact]
        public void ShouldGenerateHighDpiSettings()
        {
            var appConfigManager = new AppConfigManager();
            var actual = XDocument.Parse(appConfigManager.GenerateAppConfig(new Config.PackageConfig {
                HighDpiSupport = true
            }));
            var expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><System.Windows.Forms.ApplicationConfigurationSection><add key=\"DpiAwareness\" value=\"PerMonitorV2\" /></System.Windows.Forms.ApplicationConfigurationSection></configuration>");

            Assert.True(XNode.DeepEquals(actual, expected));
        }
    }
}
