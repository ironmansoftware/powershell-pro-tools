using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IMS.FormDesigner
{
    public class ResourceService : IResourceService, IResourceReader, IResourceWriter
    {
        public string _resourceFile;

        private Dictionary<string, object> _resources;

        public ResourceService(string resourceFile)
        {
            _resourceFile = resourceFile;
            _resources = new Dictionary<string, object>();
        }

        public void AddResource(string name, string value)
        {
            _resources.Add(name, value);
        }

        public void AddResource(string name, object value)
        {
            if (_resources.ContainsKey(name))
            {
                _resources[name] = value;
            }
            else
            {
                _resources.Add(name, value);
            }

            Generate();
        }

        public void AddResource(string name, byte[] value)
        {
            _resources.Add(name, value);
        }

        public void Close()
        {
            
        }

        public void Dispose()
        {
        }

        public void Generate()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("& { $BinaryFormatter = New-Object -TypeName System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
            stringBuilder.AppendLine(" @{ ");

            foreach (var resource in _resources)
            {
                if (resource.Value is Bitmap bitmap)
                {
                    ImageConverter converter = new ImageConverter();
                    var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
                    var content = Convert.ToBase64String(bytes);

                    stringBuilder.AppendLine($"'{resource.Key}' = New-Object -TypeName System.Drawing.Bitmap -ArgumentList @(New-Object -TypeName  System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}')))");
                }

                if (resource.Value is ImageListStreamer imageListStreamer)
                {
                    var binaryFormatter = new BinaryFormatter();
                    string content;
                    using (var memoryStream = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStream, imageListStreamer);
                        content = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    stringBuilder.AppendLine($"'{resource.Key}' = $BinaryFormatter.Deserialize((New-Object -TypeName System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}'))))");
                }

                if (resource.Value is Point point)
                {
                    stringBuilder.AppendLine($"'{resource.Key}' = New-Object -TypeName System.Drawing.Point -ArgumentList @({point.X}, {point.Y})");
                }

                if (resource.Value is Icon icon)
                {
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        icon.Save(ms);
                        bytes = ms.ToArray();
                    }

                    var content = Convert.ToBase64String(bytes);
                    stringBuilder.AppendLine($"'{resource.Key}' = New-Object -TypeName System.Drawing.Icon -ArgumentList @(New-Object -TypeName  System.IO.MemoryStream -ArgumentList @(,[System.Convert]::FromBase64String('{content}')))");
                }

                if (resource.Value is string str)
                {
                    stringBuilder.AppendLine($"'{resource.Key}' = '{str.Replace("\'", "\'\'")}'");
                }
            }

            stringBuilder.AppendLine("}");
            stringBuilder.Append("}");

            File.WriteAllText(_resourceFile, stringBuilder.ToString());
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            if (!File.Exists(_resourceFile))
            {
                return _resources.GetEnumerator();
            }

            using(var ps = PowerShell.Create())
            {
                ps.AddScript($". '{_resourceFile}'");
                var hashtable = ps.Invoke<Hashtable>().FirstOrDefault();

                if (ps.HadErrors)
                {
                    MessageBox.Show(ps.Streams.Error.First().ToString(), "Failed to Read Resource File");
                    return _resources.GetEnumerator();
                }

                if (hashtable == null)
                {
                    return _resources.GetEnumerator() ;
                }

                foreach(object key in hashtable.Keys)
                {
                    var psobject = hashtable[key] as PSObject;

                    if (hashtable[key] is string str)
                    {
                        _resources.Add(key.ToString(), str);
                    }
                    else if (psobject == null && hashtable[key] is ImageListStreamer stream)
                    {
                        _resources.Add(key.ToString(), stream);
                    }
                    else if (psobject == null)
                    {
                        var arr = hashtable[key] as object[];
                        if (arr == null) continue;

                        var imageList = new ImageList();
                        foreach (PSObject item in arr)
                        {
                            var baseObject = item.BaseObject;
                            if (baseObject is Bitmap bm)
                            {
                                imageList.Images.Add(bm);
                            }
                        }

                        _resources.Add(key.ToString(), imageList.ImageStream);
                    }
                    else
                    {
                        var value = psobject.BaseObject;
                        _resources.Add(key.ToString(), value);
                    }
                }
            }

            return _resources.GetEnumerator();
        }

        public IResourceReader GetResourceReader(CultureInfo info)
        {
            return this;
        }

        public IResourceWriter GetResourceWriter(CultureInfo info)
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}
