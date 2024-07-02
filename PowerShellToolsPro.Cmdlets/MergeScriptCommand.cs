using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsData.Merge, "Script")]
    public class MergeScriptCommand : PSCmdlet, IPathResolver
    {
        [Parameter(Mandatory = true, ParameterSetName = "Parameters", HelpMessage = "The script to package in an executable and optionally bundle with other scripts.")]
        public string Script { get; set; }
        [Parameter(ParameterSetName = "Parameters", HelpMessage = "The output path for the resulting script or executable. This should be a directory.")]
        public string OutputPath { get; set; }
        [Parameter(ParameterSetName = "Parameters", HelpMessage = "Bundles the script with dot sourced scripts found in the script.")]
        public SwitchParameter Bundle { get; set; }
        [Parameter(ParameterSetName = "Parameters", HelpMessage = "Package the script as a .NET executable.")]
        public SwitchParameter Package { get; set; }
        [Parameter(ParameterSetName = "Parameters", HelpMessage = "Obfuscate the .NET executable and PowerShell script.")]
        public SwitchParameter Obfuscate { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "Config")]
        public Hashtable Config { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "ConfigFile")]
        public string ConfigFile { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "PackageConfig")]
        public string SerializedPackageConfig { get; set; }

        public string Resolve(string path)
        {
            return GetUnresolvedProviderPathFromPSPath(path);
        }

        private static T Deserialize<T>(string body)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(body);
                writer.Flush();
                stream.Position = 0;
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
            }
        }

        private StringBuilder sb = new StringBuilder();

        protected override void EndProcessing()
        {
            if (ParameterSetName == "ConfigFile")
            {
                ExecuteWithConfigFile();
            }
            else if (ParameterSetName == "Config")
            {
                ExecuteWithConfig(Config);
            }
            else if (ParameterSetName == "PackageConfig")
            {
                var config = Deserialize<PsPackConfig>(SerializedPackageConfig);
                ExecuteWithConfig(config);
            }
            else
            {
                ExecuteWithParameters();
            }
        }

        private void ExecuteWithConfig(PsPackConfig config)
        {
            var packageProcess = new PackageProcess();
            packageProcess.Config = config;
            packageProcess.OnMessage += (sender, s) =>
            {
                WriteVerbose(s);
                sb.AppendLine(s);
            };

            packageProcess.OnErrorMessage += (sender, s) =>
            {
                WriteError(new ErrorRecord(new Exception(s), string.Empty, ErrorCategory.InvalidOperation, null));
            };

            var result = packageProcess.Execute();
            if (!result.Success)
            {
                WriteError(new ErrorRecord(new Exception(sb.ToString()), string.Empty, ErrorCategory.InvalidOperation, null));
            }
            else
            {
                WriteObject(result);
            }
        }

        private void ExecuteWithConfig(Hashtable hashtable)
        {
            PsPackConfig config;
            try
            {
                var configDeserializer = new ConfigDeserializer(this);
                config = configDeserializer.Deserialize(hashtable);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidArgument, null));
                return;
            }

            var packageProcess = new PackageProcess();
            packageProcess.Config = config;
            packageProcess.OnMessage += (sender, s) =>
            {
                WriteVerbose(s);
                sb.AppendLine(s);
            };

            packageProcess.OnErrorMessage += (sender, s) =>
            {
                WriteError(new ErrorRecord(new Exception(s), string.Empty, ErrorCategory.InvalidOperation, null));
            };

            var result = packageProcess.Execute();
            if (!result.Success)
            {
                WriteError(new ErrorRecord(new Exception(sb.ToString()), string.Empty, ErrorCategory.InvalidOperation, null));
            }
            else
            {
                WriteObject(result);
            }
        }

        private void ExecuteWithConfigFile()
        {
            var path = Resolve(ConfigFile);
            PsPackConfig config;
            try
            {
                var configDeserializer = new ConfigDeserializer(this);
                config = configDeserializer.Deserialize(path);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidArgument, null));
                return;
            }

            var packageProcess = new PackageProcess();
            packageProcess.Config = config;
            packageProcess.OnMessage += (sender, s) =>
            {
                WriteVerbose(s);
                sb.AppendLine(s);
            };

            packageProcess.OnErrorMessage += (sender, s) =>
            {
                WriteError(new ErrorRecord(new Exception(s), string.Empty, ErrorCategory.InvalidOperation, null));
            };

            var result = packageProcess.Execute();
            if (!result.Success)
            {
                WriteError(new ErrorRecord(new Exception(sb.ToString()), string.Empty, ErrorCategory.InvalidOperation, null));
            }
            else
            {
                WriteObject(result);
            }
        }

        private void ExecuteWithParameters()
        {
            ProviderInfo info;
            var paths = this.GetResolvedProviderPathFromPSPath(Script, out info);

            if (info.Name != "FileSystem")
            {
                throw new Exception("Only FileSystem provider is supported for Script.");
            }

            var outputPath = Resolve(OutputPath);

            foreach (var path in paths)
            {
                var packageProcess = new PackageProcess();
                packageProcess.Bundle = Bundle;
                packageProcess.PackAsExecutable = Package;
                packageProcess.Obfuscate = Obfuscate;
                packageProcess.OutputDirectory = outputPath;
                packageProcess.Script = path;
                packageProcess.OnMessage += (sender, s) =>
                {
                    WriteVerbose(s);
                    sb.AppendLine(s);
                };

                packageProcess.OnErrorMessage += (sender, s) =>
                {
                    WriteError(new ErrorRecord(new Exception(s), string.Empty, ErrorCategory.InvalidOperation, null));
                };

                var result = packageProcess.Execute();
                if (!result.Success)
                {
                    WriteError(new ErrorRecord(new Exception(sb.ToString()), string.Empty, ErrorCategory.InvalidOperation, null));
                }
                else
                {
                    WriteObject(result);
                }
            }
        }
    }
}
