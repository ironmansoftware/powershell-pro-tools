using PowerShellToolsPro.Packager.Config;
using System.Collections;
using Xunit;

namespace PowerShellToolsPro.Packager.Test
{
    public class ConfigDeserializerTests
    {
        [Fact]
        public void ShouldDeserializeListOfIgnoredModules()
        {
            var pathResolverMock = NSubstitute.Substitute.For<IPathResolver>();
            
            var deserializer = new ConfigDeserializer(pathResolverMock);

            var hashtable = new Hashtable();
            var bundle = new Hashtable();
            hashtable.Add("Bundle", bundle);

            bundle.Add("IgnoredModules", new[] { "1", "2" });

            var config = deserializer.Deserialize(hashtable);

            Assert.Equal(2, config.Bundle.IgnoredModules.Length);
        }

        [Fact]
        public void ShouldDeserializeTimestampServer()
        {
            var pathResolverMock = NSubstitute.Substitute.For<IPathResolver>();

            var deserializer = new ConfigDeserializer(pathResolverMock);

            var hashtable = new Hashtable();
            var package = new Hashtable();
            hashtable.Add("Package", package);

            package.Add("TimestampServer", "http://timestamp.digicert.com");

            var config = deserializer.Deserialize(hashtable);

            Assert.Equal("http://timestamp.digicert.com", config.Package.TimestampServer);
        }
    }


}
