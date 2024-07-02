using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using System;
using System.IO;
using System.Linq;
using System.Text;
#if WINDOWS
using System.Windows.Forms;
#endif
using System.Xml.Serialization;

namespace IronmanPowerShellHost
{
    internal class ResourceLoader
    {
        public static ResourceSet LoadResources()
        {
            var resource = NativeResourceManager.GetResourceFromExecutable(101, 666);

            string script;
            if (resource == null)
            {
                script = TestConfig.GetScript();
            }
            else
            {

                if (resource[0] == 239)
                {
                    resource = resource.Skip(3).ToArray();
                }

                script = Encoding.UTF8.GetString(resource.ToArray()).Trim();
            }

            var settingsResource = NativeResourceManager.GetResourceFromExecutable(102, 666);
            PackageConfig config;
            if (settingsResource == null)
            {
                config = TestConfig.GetPackageConfig();
            }
            else
            {
                var settings = Encoding.UTF8.GetString(settingsResource);
                var xs = new XmlSerializer(typeof(PackageConfig));
                config = (PackageConfig)xs.Deserialize(new StringReader(settings));
            }

            var modules = NativeResourceManager.GetResourceFromExecutable(103, 666);

            return new ResourceSet
            {
                Config = config,
                Modules = modules,
                Script = script
            };
        }
    }
}
