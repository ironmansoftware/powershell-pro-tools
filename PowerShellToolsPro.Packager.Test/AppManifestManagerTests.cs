using System.Xml.Linq;
using Xunit;

namespace PowerShellToolsPro.Packager.Test
{
    public class AppManifestManagerTests
    {
        [Fact]
        public void ShouldGenerateEmptyManifest()
        {
            var manager = new AppManifestManager();
            var actual = XDocument.Parse(manager.GenerateManifest(new Config.PackageConfig()));
            var expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\"><assemblyIdentity version=\"1.0.0.0\" name=\"MyApplication.app\"/></assembly>");

            Assert.True(XNode.DeepEquals(actual, expected));
        }

        [Fact]
        public void ShouldGenerateManifestWithRequireAdministrator()
        {
            var manager = new AppManifestManager();
            var actual = XDocument.Parse(manager.GenerateManifest(new Config.PackageConfig {
                RequireElevation = true
            }));
            var expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\"><assemblyIdentity version=\"1.0.0.0\" name=\"MyApplication.app\"/><trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v2\"><security><requestedPrivileges xmlns=\"urn:schemas-microsoft-com:asm.v3\"><requestedExecutionLevel level=\"requireAdministrator\" uiAccess=\"false\" /></requestedPrivileges></security></trustInfo></assembly>");

            Assert.True(XNode.DeepEquals(actual, expected));
        }

        [Fact]
        public void ShouldGenerateManifestWithHighDpiSettings()
        {
            var manager = new AppManifestManager();
            var actual = XDocument.Parse(manager.GenerateManifest(new Config.PackageConfig
            {
                HighDpiSupport = true
            }));
            var expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\"><assemblyIdentity version=\"1.0.0.0\" name=\"MyApplication.app\"/><compatibility xmlns=\"urn:schemas-microsoft-com:compatibility.v1\"><application><supportedOS Id=\"{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}\" /></application></compatibility></assembly>");

            Assert.True(XNode.DeepEquals(actual, expected));
        }
    }
}
