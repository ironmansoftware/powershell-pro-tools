using PowerShellToolsPro.Packager.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace PowerShellToolsPro.Packager
{
    public class Compiler
    {
        public event EventHandler<string> BuildOutput;
        public event EventHandler<string> ErrorOutput;

        private readonly IEnumerable<string> _requiredPowerShellCoreModules = new[]
        {
            "Microsoft.PowerShell.Utility",
            "Microsoft.PowerShell.Security",
            "Microsoft.PowerShell.Management",
            "Microsoft.PowerShell.Diagnostics",
            "Microsoft.PowerShell.Host",
            "CimCmdlets",
            "Microsoft.PowerShell.Archive",
            "PSDesiredStateConfiguration"
        };

        public string BuildSingleFileExecutable(StageResult previousStage, string outputDirectory, PackageConfig config, string rootFile)
        {
            var script = previousStage.OutputFileName;
            var tempDirectory = Path.Combine(outputDirectory, "bin"); // Path.GetTempPath();
            tempDirectory = Path.Combine(tempDirectory, Guid.NewGuid().ToString("N"));

            if (config.RuntimeIdentifier.Equals("win7-x64") && config.DotNetVersion.StartsWith("net8"))
            {
                config.RuntimeIdentifier = "win-x64";
            }

            OnBuildOutput("Creating temp directory: " + tempDirectory);

            var moduleDirectory = Path.Combine(tempDirectory, "Modules");
            Directory.CreateDirectory(tempDirectory);

            OnBuildOutput("Packaging modules...");

            var modules = ZipModules(moduleDirectory, previousStage, config);

            var fileInfo = new FileInfo(script);

            var projectName = fileInfo.Name.Replace("ps1", "csproj");
            var assemblyName = fileInfo.Name.Replace(".ps1", string.Empty);

            if (!string.IsNullOrEmpty(config.OutputName))
            {
                var outputName = config.OutputName;
                if (outputName.EndsWith(".exe"))
                {
                    outputName = outputName.Replace(".exe", string.Empty);
                }

                projectName = $"{outputName}.csproj";
                assemblyName = outputName;
            }

            if (!Directory.Exists(outputDirectory))
            {
                OnBuildOutput("Creating output directory: " + outputDirectory);
                Directory.CreateDirectory(outputDirectory);
            }

            OnBuildOutput("Checking dotnet version.");

            var dotnetVersion = CheckDotNetVersion(config);

            OnBuildOutput("Creating package project.");
            var xml = CreateProjectFile(config, modules, dotnetVersion);
            File.WriteAllText(Path.Combine(tempDirectory, projectName), xml);

            if (!string.IsNullOrEmpty(config.DotNetSdk))
            {
                var json = $"{{ \"sdk\": {{ \"version\": \"{config.DotNetSdk}\" }} }}";
                File.WriteAllText(Path.Combine(tempDirectory, "global.json"), json);
            }

            if (config.Resources != null)
            {
                var rootDirectory = new FileInfo(rootFile).DirectoryName;
                foreach (var resource in config.Resources)
                {
                    var sourceItem = Path.Combine(rootDirectory, resource);
                    var targetItem = Path.Combine(tempDirectory, resource);

                    var itemInfo = new FileInfo(targetItem);
                    if (!itemInfo.Directory.Exists)
                    {
                        Directory.CreateDirectory(itemInfo.DirectoryName);
                    }

                    File.Copy(sourceItem, targetItem);
                }
            }

            var scriptContents = ProcessScript(script, config);
            var key = Guid.NewGuid().ToString().Substring(0, 24);

            if (config.Obfuscate)
            {
                scriptContents = Encrypt(scriptContents, key);
            }
            File.WriteAllText(Path.Combine(tempDirectory, "script.ps1"), scriptContents, Encoding.UTF8);

            UpdateHostFile(config, tempDirectory, assemblyName, key);

            var manager = new AppManifestManager();
            var manifest = manager.GenerateManifest(config);
            File.WriteAllText(Path.Combine(tempDirectory, "app.manifest"), manifest);

            var appConfigManager = new AppConfigManager();
            var appConfig = appConfigManager.GenerateAppConfig(config);
            File.WriteAllText(Path.Combine(tempDirectory, "app.config"), appConfig);

            File.WriteAllText(Path.Combine(tempDirectory, "nuget.config"), CreateNuGetConfig());

            var result = StartDotNet("restore", tempDirectory);
            OnBuildOutput(result);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && config.RuntimeIdentifier != "linux-x64")
            {
                assemblyName += ".exe";
            }

            var resultingAssembly = Path.Combine(outputDirectory, assemblyName);

            OnBuildOutput($"Packaging {script} -> {resultingAssembly}");

            if (config.PowerShellCore)
            {
                result = StartDotNet($"publish -c Release --self-contained -r {config.RuntimeIdentifier} -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -o \"{Path.Combine(tempDirectory, "out")}\"", tempDirectory);

                var tempAssembly = Path.Combine(tempDirectory, "out", assemblyName);
                if (File.Exists(tempAssembly))
                {
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    File.Copy(tempAssembly, resultingAssembly, true);
                }
            }
            else
            {
                result = StartDotNet($"build -o \"{EscapeMsBuildChar(Path.Combine(tempDirectory, "out"))}\"", tempDirectory);

                var tempAssembly = Path.Combine(tempDirectory, "out", assemblyName);
                if (File.Exists(tempAssembly))
                {
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    File.Copy(tempAssembly, resultingAssembly, true);
                }
            }

            Directory.Delete(tempDirectory, true);

            OnBuildOutput(result);

            if (!string.IsNullOrEmpty(config.Certificate))
            {
                OnBuildOutput($"Signing assembly with certificate {config.Certificate}");
                SignAssembly(config.Certificate, resultingAssembly);
            }

            return resultingAssembly;
        }

        private static string EscapeMsBuildChar(string str)
        {
            return str.Replace("'", "%27");
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
                    throw new Exception($"Unable to sign assembly at {path}. {result.Properties["StatusMessage"].Value}");
                }
            }
        }

        private string ProcessScript(string script, PackageConfig packageConfig)
        {
            var scriptContents = File.ReadAllText(script);

            var packageType = packageConfig.PackageType;

            if (packageType == PackageType.Console)
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

            }

            return scriptContents;
        }

        private static void UpdateHostFile(PackageConfig config, string tempDirectory, string assemblyName, string key)
        {
            string host;
            if (config.PackageType == PackageType.Console)
            {
                if (config.PowerShellCore && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && config.RuntimeIdentifier != "linux-x64")
                {
                    host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Core.cs");
                }
                else if (config.PowerShellCore && RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || config.RuntimeIdentifier == "linux-x64")
                {
                    host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Linux.cs");
                }
                else if (config.PowerShellCore && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Osx.cs");
                }
                else
                {
                    if (config.HideConsoleWindow)
                    {
                        host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Hidden.cs");
                    }
                    else
                    {
                        host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost.cs");
                    }
                }
            }
            else
            {
                if (config.PowerShellCore)
                {
                    host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Service.ConsolePowerShellHost_Core.cs");
                }
                else
                {
                    host = GetResourceContent("PowerShellToolsPro.Packager.Hosts.Service.ConsolePowerShellHost.cs");
                }

                host = host.Replace("//ServiceName", config.ServiceName);
                host = host.Replace("//ServiceDisplayName", config.ServiceDisplayName);
            }

            host = host.Replace("//key", key);

            if (config.DisableQuickEdit)
            {
                host = host.Replace("//QuickEdit", "DisableQuickEdit();");
            }

            if (!string.IsNullOrEmpty(config.PowerShellArguments))
            {
                var code = new StringBuilder();
                var arguments = config.PowerShellArguments.Split(' ');
                foreach (var argument in arguments)
                {
                    code.AppendLine($"myArgs.Add(\"{argument.Replace("\"", "\\\"")}\");");
                }

                host = host.Replace("//Arguments", code.ToString());
            }

            host = host.Replace("//FileName", assemblyName.Replace(" ", "_"));
            host = host.Replace("HighDpiSetting", config.HighDpiSupport.ToString().ToLower());
            host = host.Replace("HideConsoleWindowSwitch", config.HideConsoleWindow.ToString().ToLower());

            File.WriteAllText(Path.Combine(tempDirectory, "ConsolePowerShellHost.cs"), host, Encoding.UTF8);
        }

        private static string Encrypt(string toEncrypt, string key)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            keyArray = Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        private string CreateNuGetConfig()
        {
            var nugetConfig = new XDocument(
                new XElement("configuration",
                    new XElement("packageSources",
                        new XElement("add",
                            new XAttribute("key", "NuGet"),
                            new XAttribute("value", "https://api.nuget.org/v3/index.json")
                ))
            ));

            return nugetConfig.ToString();
        }

        private string CreateProjectFile(PackageConfig config, string modules, Version dotnetVersion)
        {
            if (string.IsNullOrEmpty(config.DotNetVersion))
            {
                config.DotNetVersion = SelectHighestDotNetFramework();
            }

            if (config.HighDpiSupport && Version.TryParse(config.DotNetVersion.Trim('v'), out Version version) && version < new Version(4, 7))
            {
                OnBuildOutput("High DPI support requires .NET version 4.7 or higher.");
            }

            if (!IsDotNetFrameworkInstalled(config.DotNetVersion))
            {
                OnBuildOutput(string.Format("Failed to find the developer pack for .NET Version {0}. Your build might fail. You can install the Developer Pack from Microsoft: https://dotnet.microsoft.com/download/visual-studio-sdks", config.DotNetVersion));
            }

            if (!config.DotNetVersion.StartsWith("net"))
            {
                config.DotNetVersion = "net" + config.DotNetVersion.Trim('v').Replace(".", string.Empty);
            }

            OnBuildOutput(string.Format("Using .NET Framework version: {0}", config.DotNetVersion));

            if (string.IsNullOrEmpty(config.FileVersion))
            {
                config.FileVersion = "1.0.0.0";
            }

            var embeddedModules = new XElement("ItemGroup");
            var resources = new XElement("ItemGroup");

            if (!string.IsNullOrEmpty(modules))
            {
                embeddedModules.Add(new XElement("EmbeddedResource", new XAttribute("Include", "Modules.zip")));
            }

            embeddedModules.Add(new XElement("EmbeddedResource", new XAttribute("Include", "script.ps1")));

            if (config.Resources != null)
            {
                foreach (var resource in config.Resources)
                {
                    resources.Add(new XElement("Resource", new XAttribute("Include", resource)));
                }
            }

            var manifestNode = new XElement("PropertyGroup");
            if (config.RequireElevation && config.PackageType == PackageType.Console)
            {
                manifestNode.Add(new XElement("ApplicationManifest", "app.manifest"));
            }

            var references = new List<XElement>();
            var targets = new List<XElement>();
            if (config.PackageType == PackageType.Service)
            {
                references.Add(new XElement("ItemGroup",
                    new XElement("Reference",
                        new XAttribute("Include", "System.ServiceProcess"))
                    ));

                references.Add(new XElement("ItemGroup",
                    new XElement("Reference",
                        new XAttribute("Include", "System.Configuration.Install"))
                    ));
            }

            if (config.PowerShellCore)
            {
                references.Add(new XElement("ItemGroup",
                      new XElement("PackageReference",
                          new XAttribute("Include", "Microsoft.PowerShell.SDK"),
                          new XElement("Version", config.PowerShellVersion))
                      ));
            }
            else
            {
                references.Add(new XElement("ItemGroup",
                  new XElement("Reference",
                      new XAttribute("Include", "System.Windows.Forms"))
                  ));

                references.Add(new XElement("ItemGroup",
                    new XElement("Reference",
                        new XAttribute("Include", "Microsoft.PowerShell.ConsoleHost"),
                        new XElement("HintPath", @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Microsoft.PowerShell.ConsoleHost\v4.0_3.0.0.0__31bf3856ad364e35\Microsoft.PowerShell.ConsoleHost.dll"))
                    ));

                references.Add(new XElement("ItemGroup",
                    new XElement("Reference",
                        new XAttribute("Include", "System.Management.Automation"),
                        new XElement("HintPath", @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll"))
                    ));
            }

            var outputType = config.HideConsoleWindow && config.PackageType == PackageType.Console && config.RuntimeIdentifier != "linux-x64" ? "WinExe" : "Exe";

            var exeInfo = new XElement("PropertyGroup",
                    new XElement("OutputType", outputType),
                    new XElement("FileVersion", config.FileVersion),
                    new XElement("Company", config.CompanyName),
                    new XElement("Copyright", config.Copyright),
                    new XElement("AssemblyTitle", config.FileDescription),
                    new XElement("Product", config.ProductName),
                    new XElement("InformationalVersion", config.ProductVersion),
                    new XElement("ApplicationIcon", config.ApplicationIconPath)
                    );

            var defineConstantsNode = new XElement("PropertyGroup");
            if (config.Obfuscate)
            {
                defineConstantsNode.Add(new XElement("DefineConstants", "$(DefineConstants);OBFUSCATE"));
            }

            if (config.PowerShellCore)
            {
                var dotnetversion = config.DotNetVersion;

                if (config.Lightweight)
                {
                    OnBuildOutput("Lightweight enabled. Not including Windows Forms or WPF.");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && config.RuntimeIdentifier != "linux-x64" && !config.Lightweight)
                {
                    if (dotnetversion == "net5.0" || dotnetversion == "net6.0" || dotnetversion == "net7.0" || dotnetversion == "net8.0" || dotnetversion == "net9.0")
                    {
                        dotnetversion += "-windows";
                    }
                    exeInfo.Add(new XElement("UseWindowsForms", "true"));
                    exeInfo.Add(new XElement("UseWPF", "true"));
                }

                exeInfo.Add(new XElement("TargetFramework", dotnetversion));

                if (config.Obfuscate)
                {
                    //var obfuscateTarget = new XElement("Target", new XAttribute("AfterTargets", "AfterCompile"),
                    //    new XElement("Exec", new XAttribute("command", "obfuscar.console.exe obfuscar.xml")),
                    //    new XElement("Exec", new XAttribute("Command", "COPY $(ProjectDir)$(IntermediateOutputPath)Obfuscated\\$(TargetFileName) $(ProjectDir)$(IntermediateOutputPath)$(TargetFileName)")));

                    //targets.Add(obfuscateTarget);
                }
            }
            else
            {
                exeInfo.Add(new XElement("TargetFramework", config.DotNetVersion));
                exeInfo.Add(new XElement("Prefer32Bit", config.Platform?.ToLower() == "x64" ? "false" : "true"));
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    exeInfo.Add(new XElement("UseWPF", "true"));
                }
            }

            var sdk = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && dotnetVersion.Major < 5 && config.RuntimeIdentifier != "linux-x64" ? "Microsoft.NET.Sdk.WindowsDesktop" : "Microsoft.NET.Sdk";

            var project = new XElement("Project", new XAttribute("Sdk", sdk),
                exeInfo,
                defineConstantsNode,
                references.ToArray(),
                manifestNode,
                embeddedModules,
                resources,
                targets.ToArray()
            );

            var csproj = new XDocument(project);
            return csproj.ToString();
        }

        private static string FindExePath(string exe)
        {
            var fileInfo = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);

            if (fileInfo.Name.Equals(exe, StringComparison.OrdinalIgnoreCase))
            {
                return fileInfo.FullName;
            }

            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty)
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                throw new FileNotFoundException(new FileNotFoundException().Message + exe, exe);
            }
            return Path.GetFullPath(exe);
        }

        private string ZipModules(string stagingDirectory, StageResult previousStage, PackageConfig config)
        {
            if (config.PowerShellCore)
            {
                /*
                 * For the next time I look at this: We probably can figure out how to not do this but with a single file executable
                 * it will not include native binaries. The modules for pwsh are in the runtimes folder so they dont get included
                 * when powershell starts up it tries to load the utlities module, finds it in the windows powershell system folder,
                 * attempts to load it with wincompat and then hangs
                 */

                var exe = "pwsh.exe";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    exe = "pwsh";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    exe = "pwsh";
                }

                var pwsh = new FileInfo(FindExePath(exe));
                var moduleDirectory = Path.Combine(pwsh.DirectoryName, "Modules");

                foreach (var module in _requiredPowerShellCoreModules)
                {
                    var path = Path.Combine(moduleDirectory, module, $"{module}.psd1");
                    var fileInfo = new FileInfo(path);
                    if (!fileInfo.Directory.Exists)
                    {
                        continue;
                    }

                    var modDir = Path.Combine(stagingDirectory, module);

                    CopyFolder(fileInfo.Directory.FullName, modDir);
                }
            }

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

        private Version CheckDotNetVersion(PackageConfig config)
        {
            OnBuildOutput("Checking dotnet SDK version.");
            var versionNumber = StartDotNet("--version", null);
            OnBuildOutput(".NET SDK Version: " + versionNumber);

            // Accounts for semantic versioning...
            var versionPart = versionNumber.Split('-')[0];

            Version installedVersion;
            Version minVersion = new Version(1, 0, 0);

            if (config.PowerShellCore)
            {
                minVersion = new Version(2, 2, 0);
            }

            if (!Version.TryParse(versionPart, out installedVersion))
            {
                throw new InvalidDotNetVersionException(versionNumber, minVersion);
            }

            if (installedVersion < minVersion)
            {
                throw new InvalidDotNetVersionException(installedVersion, minVersion);
            }

            if (installedVersion.Major == 5)
            {
                OnBuildOutput("There is a known issue with compiling executables using .NET 5.x. If you have issues with your executable, consider using the .NET 3.x or 6.x tools instead.");
            }

            return installedVersion;
        }

        private string StartDotNet(string arguments, string workingDirectory, string exe = "dotnet", bool installDotnet = true)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo(exe, arguments);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            try
            {
                process.Start();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 2)
                {
                    if (installDotnet)
                    {
                        OnBuildOutput("dotnet not found. Installing latest version of dotnet.");
                        var client = new HttpClient();
                        var dotnetInstall = client.GetStringAsync("https://dot.net/v1/dotnet-install.ps1").GetAwaiter().GetResult();
                        var dotnetInstallPath = Path.Combine(Path.GetTempPath(), "dotnet-install.ps1");
                        File.WriteAllText(dotnetInstallPath, dotnetInstall);

                        using (var ps = PowerShell.Create())
                        {
                            ps.AddScript($". '{dotnetInstallPath}'");

                            try
                            {
                                ps.Invoke();
                                if (ps.HadErrors)
                                {
                                    foreach (var error2 in ps.Streams.Error)
                                    {
                                        OnBuildError("[ERROR] " + error2.ToString());
                                    }
                                }
                                else
                                {
                                    StartDotNet(arguments, workingDirectory, exe, false);
                                }
                            }
                            catch (Exception ex2)
                            {
                                throw new Exception($"Failed to install dotnet. {ex2.Message} You can download it from Microsoft. https://dotnet.microsoft.com/download or run https://dot.net/v1/dotnet-install.ps1");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"dotnet is not installed and required. You can download it from Microsoft. https://dotnet.microsoft.com/download or run https://dot.net/v1/dotnet-install.ps1");
                    }

                }
                throw;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            OnBuildOutput(output);

            if (process.ExitCode != 0)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    OnBuildError(error);
                }

                throw new Exception("Failed to build.");
            }

            return output;
        }

        private static string GetResourceContent(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resource))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private void OnBuildOutput(string output)
        {
            if (BuildOutput != null)
                BuildOutput(this, output);
        }

        private void OnBuildError(string output)
        {
            if (ErrorOutput != null)
                ErrorOutput(this, output);
        }


        private string SelectHighestDotNetFramework()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var referenceAssemblies = Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework");

            if (!Directory.Exists(referenceAssemblies))
            {
                OnBuildOutput("Failed to find the Reference Assemblies folder. The build may fail. Make sure you have the .NET Developer Pack installed. https://www.microsoft.com/en-us/download/details.aspx?id=53321 ");
                return "4.6.2";
            }

            Version highestVersion = new Version(1, 1);
            foreach (var folder in Directory.GetDirectories(referenceAssemblies))
            {
                var directoryInfo = new DirectoryInfo(folder);

                var folderVersion = directoryInfo.Name.TrimStart('v');
                Version version;
                if (!Version.TryParse(folderVersion, out version))
                    continue;

                if (version > highestVersion)
                    highestVersion = version;
            }

            if (highestVersion == new Version(1, 1))
            {
                OnBuildOutput("Failed to find the Reference Assemblies folder. The build may fail. Make sure you have the .NET Developer Pack installed. https://www.microsoft.com/en-us/download/details.aspx?id=53321 ");
                return "4.6.2";
            }

            return highestVersion.ToString();
        }

        private bool IsDotNetFrameworkInstalled(string version)
        {
            if (version.StartsWith("netcoreapp") || version == "net5.0" || version == "net6.0" || version == "net7.0" || version == "net8.0" || version == "net9.0") return true;

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var referenceAssemblies = Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework");

            version = version.TrimStart('v');

            if (!Directory.Exists(referenceAssemblies))
            {
                OnBuildOutput("Failed to find the Reference Assemblies folder. The build may fail. Make sure you have the .NET Developer Pack installed. https://www.microsoft.com/en-us/download/details.aspx?id=53321 ");
                return false;
            }

            if (!Version.TryParse(version, out Version targetVersion))
            {
                return false;
            }

            foreach (var folder in Directory.GetDirectories(referenceAssemblies))
            {
                var directoryInfo = new DirectoryInfo(folder);

                var folderVersion = directoryInfo.Name.TrimStart('v');
                if (!Version.TryParse(folderVersion, out Version parsedVersion))
                    continue;

                if (targetVersion == parsedVersion)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class DotNetNotInstalledException : Exception
    {

    }

    public class InvalidDotNetVersionException : Exception
    {
        public Version InstalledVersion { get; set; }
        public Version MinimumVersion { get; set; }

        public InvalidDotNetVersionException(Version installedVersion, Version minVersion) : base(
            MakeMessage(installedVersion, minVersion))
        {
            InstalledVersion = installedVersion;
            MinimumVersion = minVersion;
        }

        public InvalidDotNetVersionException(string installedVersion, Version minVersion) : base(
            MakeMessage(installedVersion, minVersion))
        {
            MinimumVersion = minVersion;
        }

        private static string MakeMessage(string installedVersion, Version minVersion)
        {
            return $".NET Core SDK version {minVersion} required but {installedVersion} installed.";
        }

        private static string MakeMessage(Version installedVersion, Version minVersion)
        {
            return $".NET Core SDK version {minVersion} required but {installedVersion} installed.";
        }
    }
}
