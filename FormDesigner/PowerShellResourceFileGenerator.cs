using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace IMS.FormDesigner
{
    public class PowerShellResourceFileGenerator
    {
        private const string BitmapType = "System.Drawing.Bitmap";
        private const string IconType = "System.Drawing.Icon";

        public void GenerateCode(XDocument resx, string inputFile, TextWriter writer)
        {
            writer.WriteLine("& { $BinaryFormatter = New-Object -TypeName System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");

            var dataItems = resx.Descendants("data");
            var fileInfo = new FileInfo(inputFile);
            var fileRoot = fileInfo.DirectoryName;
            var parsedDataItems = new Dictionary<string, string>();
            foreach (var data in dataItems)
            {
                var key = data.Attribute("name").Value;
                var type = data.Attribute("type")?.Value;

                var mimetype = data.Attribute("mimetype")?.Value;
                string content = null;

                string finalResourceData = string.Empty;
                if (string.IsNullOrEmpty(mimetype))
                {
                    var value = data.Element("value").Value;
                    var resourceData = value.Split(';')[0];
                    var resourceType = value.Split(';')[1];
                    var filePath = Path.Combine(fileRoot, resourceData);
                    var imageBytes = File.ReadAllBytes(filePath);
                    content = Convert.ToBase64String(imageBytes);
                }
                else if (mimetype.Contains("base64"))
                {
                    content = data.Element("value").Value.Trim().Replace("\n        ", "");
                }

                if (content == null)
                    continue;

                if (type == null && key.Contains("ImageStream"))
                {
                    var variable = "$" + key.Replace(".", "_");
                    writer.WriteLine($"{variable} = $BinaryFormatter.Deserialize((New-Object -TypeName System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}'))))");
                    finalResourceData = variable;
                }
                else if (type == null)
                {
                    continue;
                }
                else if (type.StartsWith(BitmapType))
                {
                    finalResourceData = $"New-Object -TypeName System.Drawing.Bitmap -ArgumentList @(New-Object -TypeName  System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}')))";
                }
                else if (type.StartsWith(IconType))
                {
                    finalResourceData = $"New-Object -TypeName System.Drawing.Icon -ArgumentList @(New-Object -TypeName  System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}')))";
                }

                parsedDataItems.Add(key, finalResourceData);
            }

            writer.WriteLine("@{");

            foreach (var parsedItem in parsedDataItems)
            {
                writer.WriteLine("'" + parsedItem.Key + "' = " + parsedItem.Value + ";");
            }

            writer.WriteLine("}");
            writer.Write("}");
        }

    }
}
