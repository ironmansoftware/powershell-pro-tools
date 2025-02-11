using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerShellToolsPro.Packager.Config
{
    public class ConfigDeserializer
    {
        private readonly IPathResolver _pathResolver;

        public ConfigDeserializer(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public PsPackConfig Deserialize(Hashtable configTable)
        {
            var config = new PsPackConfig();
            config.Root = ReadScalarValue<string>(configTable, "Root");
            config.Root = _pathResolver.Resolve(config.Root);

            config.OutputPath = ReadScalarValue<string>(configTable, "OutputPath");
            config.OutputPath = _pathResolver.Resolve(config.OutputPath);

            var packagerHashtable = ReadScalarValue<Hashtable>(configTable, "Package");
            if (packagerHashtable != null)
            {
                config.Package.Enabled = ReadScalarValue<bool>(packagerHashtable, "Enabled");
                config.Package.DotNetVersion = ReadScalarValue<string>(packagerHashtable, "DotNetVersion");
                config.Package.HideConsoleWindow = ReadScalarValue<bool>(packagerHashtable, "HideConsoleWindow");
                config.Package.Obfuscate = ReadScalarValue<bool>(packagerHashtable, "Obfuscate");
                config.Package.ApplicationIconPath = ReadScalarValue<string>(packagerHashtable, "ApplicationIconPath");
                config.Package.PackageType = (PackageType)Enum.Parse(typeof(PackageType), ReadScalarValue<string>(packagerHashtable, "PackageType", "Console"));
                config.Package.Host = (PowerShellHosts)Enum.Parse(typeof(PowerShellHosts), ReadScalarValue<string>(packagerHashtable, "Host", "Default"));
                config.Package.ServiceDisplayName = ReadScalarValue<string>(packagerHashtable, "ServiceDisplayName");
                config.Package.ServiceName = ReadScalarValue<string>(packagerHashtable, "ServiceName");
                config.Package.RequireElevation = ReadScalarValue<bool>(packagerHashtable, "RequireElevation");
                config.Package.RuntimeIdentifier = ReadScalarValue<string>(packagerHashtable, "RuntimeIdentifier", "win7-x64");
                config.Package.PowerShellVersion = ReadScalarValue<string>(packagerHashtable, "PowerShellVersion", "Windows PowerShell");
                config.Package.Certificate = ReadScalarValue<string>(packagerHashtable, "Certificate");

                config.Package.FileVersion = ReadScalarValue<string>(packagerHashtable, "FileVersion");
                config.Package.FileDescription = ReadScalarValue<string>(packagerHashtable, "FileDescription");
                config.Package.CompanyName = ReadScalarValue<string>(packagerHashtable, "CompanyName");
                config.Package.ProductName = ReadScalarValue<string>(packagerHashtable, "ProductName");
                config.Package.ProductVersion = ReadScalarValue<string>(packagerHashtable, "ProductVersion");
                config.Package.Copyright = ReadScalarValue<string>(packagerHashtable, "Copyright");
                config.Package.HighDpiSupport = ReadScalarValue<bool>(packagerHashtable, "HighDpiSupport");
                config.Package.PowerShellArguments = ReadScalarValue<string>(packagerHashtable, "PowerShellArguments");
                config.Package.Platform = ReadScalarValue<string>(packagerHashtable, "Platform");
                config.Package.DisableQuickEdit = ReadScalarValue<bool>(packagerHashtable, "DisableQuickEdit");
                config.Package.Resources = ReadScalarValue<string[]>(packagerHashtable, "Resources");
                config.Package.DotNetSdk = ReadScalarValue<string>(packagerHashtable, "DotNetSdk");
                config.Package.Certificate = ReadScalarValue<string>(packagerHashtable, "Certificate");
                config.Package.OutputName = ReadScalarValue<string>(packagerHashtable, "OutputName");
                config.Package.Lightweight = ReadScalarValue<bool>(packagerHashtable, "Lightweight");
            }

            var bundlerHashtable = ReadScalarValue<Hashtable>(configTable, "Bundle");
            if (bundlerHashtable != null)
            {
                config.Bundle.Enabled = ReadScalarValue<bool>(bundlerHashtable, "Enabled");
                config.Bundle.RequiredAssemblies = ReadScalarValue<bool>(bundlerHashtable, "RequiredAssemblies");
                config.Bundle.Modules = ReadScalarValue<bool>(bundlerHashtable, "Modules");
                config.Bundle.NestedModules = ReadScalarValue<bool>(bundlerHashtable, "NestedModules");
                config.Bundle.IgnoredModules = ReadScalarValue<string[]>(bundlerHashtable, "IgnoredModules");
            }

            return config;
        }

        public PsPackConfig Deserialize(string file)
        {
            if (!File.Exists(file))
                throw new Exception(string.Format("Config file {0} does not exist.", file));

            var contents = File.ReadAllText(file);

            Hashtable configTable;
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Import-PowerShellDataFile").AddParameter("Path", file);
                configTable = ps.Invoke<Hashtable>().FirstOrDefault();

                if (ps.HadErrors)
                {
                    throw ps.Streams.Error.First().Exception;
                }
            }

            return Deserialize(configTable);
        }

        private static T ReadScalarValue<T>(Hashtable table, string key, T defaultValue = default(T))
        {
            try
            {
                if (!table.ContainsKey(key))
                {
                    return defaultValue;
                }

                var value = table[key];
                if (value is PSObject pSObject)
                {
                    return (T)pSObject.BaseObject;
                }

                if (typeof(T) == typeof(string))
                {
                    var stringValue = value as string;
                    if (stringValue == null)
                    {
                        return defaultValue;
                    }

                    value = Environment.ExpandEnvironmentVariables(stringValue);

                    return (T)value;
                }

                return (T)value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read '{key}'. Expected '{typeof(T)}' but was '{table[key].GetType()}'.", ex);
            }

        }
    }
}
