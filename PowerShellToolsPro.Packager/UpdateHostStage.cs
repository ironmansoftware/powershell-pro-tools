using AsmResolver;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources;
using AsmResolver.PE.Win32Resources.Builder;
using AsmResolver.PE.Win32Resources.Icon;
using AsmResolver.PE.Win32Resources.Version;
using PowerShellToolsPro.Packager.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using PSPackager;
using System.Security.Cryptography.X509Certificates;

namespace PowerShellToolsPro.Packager
{
    public class UpdateHostStage : Stage
    {
        public override StageResult Execute(PackageProcess process, StageResult previousStage)
        {
            var hostDownloadService = new HostDownloadService();
            var localHosts = hostDownloadService.GetLocalHosts();

            var hostPath = localHosts.FirstOrDefault(m => m.GetId() == process.Config.Package.Host)?.FileName;

            WriteDebugMessage($"Generating assembly");

            if (string.IsNullOrEmpty(hostPath))
            {
                var parentPath = Path.GetDirectoryName(GetType().Assembly.Location);
                hostPath = Path.Combine(parentPath, "Hosts");
                switch (process.Config.Package.Host)
                {
                    case PowerShellHosts.IronmanPowerShellHost:
                    case PowerShellHosts.IronmanPowerShellWinFormsHost:
                        hostPath = Path.Combine(hostPath, "IronmanPowerShellHost.exe");
                        break;
                    default:
                        return new StageResult(false);
                }
            }

            var targetFileName = Path.GetFileNameWithoutExtension(previousStage.OutputFileName) + ".exe";
            if (!string.IsNullOrEmpty(process.Config.Package.OutputName))
            {
                targetFileName = process.Config.Package.OutputName;
            }

            var targetPath = Path.Combine(process.OutputDirectory, targetFileName);
            File.Copy(hostPath, targetPath, true);

            if (!string.IsNullOrEmpty(process.Config.Package.ApplicationIconPath) && File.Exists(process.Config.Package.ApplicationIconPath))
            {
                var iconChanger = new IconChanger();
                iconChanger.ChangeIcon(targetPath, process.Config.Package.ApplicationIconPath);
            }

            var peImage = PEImage.FromFile(targetPath);
            peImage.Resources.AddOrReplaceEntry(new ResourceDirectory(666));

            AddScript(peImage, previousStage, process);
            AddSettings(peImage, process);
            AddModules(peImage, previousStage, process);
            AddVersionInfo(peImage, process.Config);
            ReplaceManifest(peImage, process.Config.Package);

            if (process.Config.Package.Host == PowerShellHosts.IronmanPowerShellWinFormsHost)
            {
                peImage.SubSystem = SubSystem.WindowsGui;
            }
            else
            {
                peImage.SubSystem = SubSystem.WindowsCui;
            }

            WritePeImage(peImage, targetPath);

            if (!string.IsNullOrEmpty(process.Config.Package.Certificate))
            {
                WriteDebugMessage($"Signing assembly with certificate {process.Config.Package.Certificate}");
                SignAssembly(process.Config.Package.Certificate, targetPath);
            }

            return new StageResult(targetPath, true);
        }

        private void SignAssembly(string certificate, string path)
        {
            X509Certificate2 cert = null;
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-Item").AddParameter("Path", certificate);
                cert = ps.Invoke<X509Certificate2>().FirstOrDefault();
                if (cert == null)
                {
                    throw new Exception($"Unable to find certificate at {certificate}.");
                }
            }

            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Set-AuthenticodeSignature")
                    .AddParameter("FilePath", path)
                    .AddParameter("Certificate", cert);
                var result = ps.Invoke().First();

                if (ps.HadErrors)
                {
                    throw new Exception($"Unable to sign assembly at {path}. {ps.Streams.Error.First()}");
                }

                if (result.Properties["Status"].Value.ToString() != "Valid")
                {
                    throw new Exception(
                        $"Unable to sign assembly at {path}. {result.Properties["StatusMessage"].Value}");
                }
            }
        }

        void WritePeImage(IPEImage peImage, string targetPath)
        {
            var builder = new ManagedPEFileBuilder();
            var newPEFile = builder.CreateFile(peImage);

            using (var stream = File.Create(targetPath))
            {
                var writer = new BinaryStreamWriter(stream);
                newPEFile.Write(writer);
            }
        }

        void ReplaceManifest(IPEImage peImage, PackageConfig config)
        {
            var manifestManager = new AppManifestManager();
            var manifest = manifestManager.GenerateManifest(config);

            var manifestResource = new ResourceData(id: 0, contents: new DataSegment(Encoding.UTF8.GetBytes(manifest)));

            peImage.Resources.GetDirectory(ResourceType.Manifest).GetDirectory(1).AddOrReplaceEntry(manifestResource);
        }

        void AddScript(IPEImage peImage, StageResult previousStage, PackageProcess process)
        {
            var scriptContents = File.ReadAllBytes(previousStage.OutputFileName);

            if (scriptContents[0] == 239)
            {
                scriptContents = scriptContents.Skip(3).ToArray();
            }

            var scriptString = Encoding.UTF8.GetString(scriptContents);

            scriptString = ProcessScript(scriptString, process.Config.Package);

            var script = new ResourceData(id: 101, contents: new DataSegment(Encoding.UTF8.GetBytes(scriptString)));

            peImage.Resources.GetDirectory(666).AddOrReplaceEntry(script);
        }

        private string ProcessScript(string scriptContents, PackageConfig packageConfig)
        {
            var scriptBlock = ScriptBlock.Create(scriptContents);
            var scriptBlockAst = scriptBlock.Ast as ScriptBlockAst;
            if (scriptBlockAst.ParamBlock != null)
            {
                var paramBlockAst = scriptBlockAst.ParamBlock.ToString();
                var parenIndex = paramBlockAst.LastIndexOf(")");
                var firstParenIndex = paramBlockAst.IndexOf("(");

                var additionalText = "$PoshToolsRoot";
                if (firstParenIndex != (parenIndex - 1))
                {
                    additionalText = ", " + additionalText;
                }

                scriptContents = scriptContents.Insert(scriptBlockAst.ParamBlock.Extent.StartOffset + parenIndex, additionalText);
            }
            else
            {
                var startIndex = 0;
                if (scriptBlockAst.UsingStatements != null && scriptBlockAst.UsingStatements.Any())
                {
                    startIndex = scriptBlockAst.UsingStatements.Last().Extent.EndOffset + 1;
                }

                scriptContents = scriptContents.Insert(startIndex, "\r\nparam($PoshToolsRoot)\r\n");
            }

            scriptBlock = ScriptBlock.Create(scriptContents);
            scriptBlockAst = scriptBlock.Ast as ScriptBlockAst;

            if (packageConfig.HighDpiSupport && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                scriptContents = scriptContents.Insert(scriptBlockAst.ParamBlock.Extent.EndOffset, "[void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms'); [System.Windows.Forms.Application]::EnableVisualStyles(); \r\n");
            }

            scriptBlock = ScriptBlock.Create(scriptContents);
            scriptBlockAst = scriptBlock.Ast as ScriptBlockAst;

            if (scriptBlockAst.UsingStatements?.Any() == true)
            {
                var lastUsing = scriptBlockAst.UsingStatements.Last().Extent.EndOffset;
                scriptContents = scriptContents.Insert(lastUsing + 1, "& {") + Environment.NewLine + "}";
            }
            else
            {
                scriptContents = scriptContents.Insert(0, "& {") + Environment.NewLine + "}";
            }

            return scriptContents;
        }


        void AddSettings(IPEImage peImage, PackageProcess process)
        {
            var xs = new XmlSerializer(typeof(PackageConfig));
            var stringWriter = new StringWriter();
            xs.Serialize(stringWriter, process.Config.Package);

            var settings = new ResourceData(id: 102, contents: new DataSegment(Encoding.UTF8.GetBytes(stringWriter.ToString())));
            peImage.Resources.GetDirectory(666).AddOrReplaceEntry(settings);
        }

        void AddModules(IPEImage peImage, StageResult previousStage, PackageProcess process)
        {
            var tempDirectory = Path.GetTempPath();
            tempDirectory = Path.Combine(tempDirectory, Guid.NewGuid().ToString("N"));

            var moduleDirectory = Path.Combine(tempDirectory, "Modules");
            Directory.CreateDirectory(tempDirectory);

            var modules = ZipModules(moduleDirectory, previousStage, process.Config.Package);

            if (!string.IsNullOrEmpty(modules))
            {
                var modulesData = new ResourceData(id: 103, contents: new DataSegment(File.ReadAllBytes(modules)));
                peImage.Resources.GetDirectory(666).AddOrReplaceEntry(modulesData);
            }
        }

        void AddVersionInfo(IPEImage peImage, PsPackConfig config)
        {
            var versionInfo = VersionInfoResource.FromDirectory(peImage.Resources);

            if (!string.IsNullOrEmpty(config.Package.FileVersion))
            {
                if (config.Package.FileVersion.Where(m => m == '.').Count() == 0)
                {
                    config.Package.FileVersion += ".0.0.0";
                }

                if (config.Package.FileVersion.Where(m => m == '.').Count() == 1)
                {
                    config.Package.FileVersion += ".0.0";
                }

                if (config.Package.FileVersion.Where(m => m == '.').Count() == 2)
                {
                    config.Package.FileVersion += ".0";
                }

                if (!Version.TryParse(config.Package.FileVersion, out Version version))
                {
                    version = new Version(1, 0, 0, 0);
                }

                versionInfo.FixedVersionInfo.FileVersion = version;
            }


            var stringInfo = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            var stringTable = stringInfo.Tables[0];

            if (config.Package.CompanyName != null)
                stringTable[StringTable.CompanyNameKey] = config.Package.CompanyName;

            if (config.Package.FileDescription != null)
                stringTable[StringTable.FileDescriptionKey] = config.Package.FileDescription;

            if (config.Package.ProductName != null)
                stringTable[StringTable.ProductNameKey] = config.Package.ProductName;

            if (config.Package.ProductVersion != null)
                stringTable[StringTable.ProductVersionKey] = config.Package.ProductVersion;

            if (config.Package.Copyright != null)
                stringTable[StringTable.LegalCopyrightKey] = config.Package.Copyright;

            versionInfo.WriteToDirectory(peImage.Resources);
        }

        private string ZipModules(string stagingDirectory, StageResult previousStage, PackageConfig config)
        {
            if (previousStage.Data.ContainsKey("Modules"))
            {
                var psmoduleInfos = previousStage.Data["Modules"] as List<ModuleInfo>;
                if (psmoduleInfos != null && psmoduleInfos.Any())
                {
                    foreach (var moduleInfo in psmoduleInfos.DistinctBy(m => m.Name.ToLower()))
                    {
                        var directoryInfo = new DirectoryInfo(moduleInfo.ModuleBase);
                        if (!directoryInfo.Exists)
                        {
                            continue;
                        }

                        var moduleDirectory = Path.Combine(stagingDirectory, moduleInfo.Name);

                        CopyFolder(directoryInfo.FullName, moduleDirectory);

                        if (moduleInfo.NestedModules != null)
                        {
                            foreach (var nestedModule in moduleInfo.NestedModules)
                            {
                                var path = nestedModule.Path;
                                if (Path.IsPathRooted(path) && path.EndsWith(".dll"))
                                {
                                    var fi = new FileInfo(path);
                                    var targetFile = Path.Combine(moduleDirectory, fi.Name);

                                    if (!File.Exists(targetFile))
                                    {
                                        File.Copy(path, targetFile);
                                    }
                                }
                                else if (path.EndsWith(".dll"))
                                {
                                    var gacModule = GacInfo.GetAssemblies(4).FirstOrDefault(m => m.Assembly.Name.Equals(path.Replace(".dll", string.Empty)));
                                    if (gacModule != null)
                                    {
                                        File.Copy(gacModule.Path, moduleDirectory);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var stagedZip = Path.Combine(stagingDirectory, "..", "Modules.zip");

            if (Directory.Exists(stagingDirectory))
            {
                bool success = false;
                var retries = 0;
                while (!success)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        ZipFile.CreateFromDirectory(stagingDirectory, stagedZip, CompressionLevel.Optimal, false);
                        success = true;
                    }
                    catch
                    {
                        if (retries > 10)
                        {
                            return null;
                        }
                        retries++;
                    }
                }

                Directory.Delete(stagingDirectory, true);

                return stagedZip;
            }

            return null;
        }

        static public void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);

                var fileInfo = new FileInfo(file);
                if (fileInfo.Name.Equals("System.Management.Automation.dll", StringComparison.OrdinalIgnoreCase)) continue;

                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public override bool ShouldExecute(PackageProcess process, StageResult previousStage)
        {
            return process.Config.Package.Host != PowerShellHosts.Default;
        }
    }
}
