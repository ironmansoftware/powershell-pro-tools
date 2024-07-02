using PowerShellToolsPro.Packager.Config;
using System.IO;
using System.Xml.Linq;

namespace PowerShellToolsPro.Packager
{
    public class AppManifestManager
    {
        public string GenerateManifest(PackageConfig packageConfig)
        {
            XNamespace nsSys = "urn:schemas-microsoft-com:asm.v1";
            XNamespace nsSysv2 = "urn:schemas-microsoft-com:asm.v2";
            XNamespace nsSysv3 = "urn:schemas-microsoft-com:asm.v3";
            XNamespace nsCompatV1 = "urn:schemas-microsoft-com:compatibility.v1";

            XElement requireAdminNode = null;
            if (packageConfig.RequireElevation)
            {
                requireAdminNode = new XElement(nsSysv2 + "trustInfo",
                    new XElement(nsSysv2 + "security",
                        new XElement(nsSysv3 + "requestedPrivileges",
                            new XElement(nsSysv3 + "requestedExecutionLevel", new XAttribute("level", "requireAdministrator"), new XAttribute("uiAccess", "false")))));
            }

            XElement compatNode = null;
            if (packageConfig.HighDpiSupport)
            {
                // Only supports windows 10
                compatNode = new XElement(nsCompatV1 + "compatibility",
                    new XElement(nsCompatV1 + "application",
                        new XElement(nsCompatV1 + "supportedOS", new XAttribute("Id", "{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}"))));
            }

            var doc = new XDocument(new XElement(nsSys + "assembly", new XAttribute("manifestVersion", "1.0"),
                new XElement(nsSys + "assemblyIdentity", new XAttribute("version", "1.0.0.0"), new XAttribute("name", "MyApplication.app")),
                requireAdminNode,
                compatNode));

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
