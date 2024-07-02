using PowerShellToolsPro.Packager.Config;
using System.IO;
using System.Xml.Linq;

namespace PowerShellToolsPro.Packager
{
    public class AppConfigManager
    {
        public string GenerateAppConfig(PackageConfig config)
        {
            XElement highDpiNode = null;
            if (config.HighDpiSupport)
            {
                highDpiNode = new XElement("System.Windows.Forms.ApplicationConfigurationSection",
                    new XElement("add", new XAttribute("key", "DpiAwareness"), new XAttribute("value", "PerMonitorV2")));
            }

            var doc = new XDocument(new XElement("configuration", highDpiNode));

            doc.Declaration = new XDeclaration("1.0", "utf-8", null);

            string result;

            using (StringWriter writer = new StringWriter())
            {
                doc.Save(writer);
                result = writer.ToString().Replace("utf-16", "utf-8");
            }

            return result;
        }
    }
}
