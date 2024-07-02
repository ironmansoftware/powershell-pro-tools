using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
    public class ModuleCompilerTests : IDisposable
    {
        private List<string> _filesToDelete = new List<string>();
        private List<string> _messages = new List<string>();

        private PsPackConfig MakeConfig(string root, string outputDir)
        {
            return new PsPackConfig
            {
                OutputPath = outputDir,
                Root = root,
                Bundle =
                {
                    Enabled = true,
                    Modules = true,
                    NestedModules = true,
                    RequiredAssemblies = true
                },
                Package = new PackageConfig
                {
                    Enabled = true
                }

            };
        }


        [Fact]
        public void ShouldBundleIronmanHost()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "Get-ChildItem");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.Host = PowerShellHosts.IronmanPowerShellHost;
            packager.Config.Package.CompanyName = "Ironman Software, LLC";
            packager.Config.Package.ProductName = "Packager";
            packager.Config.Package.FileVersion = "2.0.0";
            packager.Config.Package.ApplicationIconPath = "C:\\Users\\adamr\\Desktop\\favicon.ico";
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact]
        public void ShouldHaveScriptRoot()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "$PSScriptRoot");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.Host = PowerShellHosts.IronmanPowerShellHost;
            packager.Config.Package.CompanyName = "Ironman Software, LLC";
            packager.Config.Package.ProductName = "Packager";
            packager.Config.Package.FileVersion = "2.0.0";
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact]
        public void ShouldBundleIronmanHostUnicode()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "ëÀØÆÆ");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.Host = PowerShellHosts.IronmanPowerShellHost;
            packager.Config.Package.CompanyName = "Ironman Software, LLC";
            packager.Config.Package.ProductName = "Packager";
            packager.Config.Package.FileVersion = "2.0.0";
            packager.Config.Package.ApplicationIconPath = "C:\\Users\\adamr\\Desktop\\favicon.ico";
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact]
        public void ShouldBundleIronmanHostWinForms()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[System.Windows.Forms.MessageBox]::Show('Hello')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.Host = PowerShellHosts.IronmanPowerShellWinFormsHost;
            packager.Config.Package.CompanyName = "Ironman Software, LLC";
            packager.Config.Package.ProductName = "Packager";
            packager.Config.Package.FileVersion = "2.0.0";
            packager.Config.Package.ApplicationIconPath = "C:\\Users\\adamr\\Desktop\\favicon.ico";
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact]
        public void ShouldBundleIronmanHostModules()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var tempFile = Path.GetTempFileName();
            var rootScript = CreateScript(moduleDirectory, $"$ENV:PSModulePath\r\nImport-Module -Name {manifest.FullName}\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.Host = PowerShellHosts.IronmanPowerShellHost;
            // packager.Config.Package.RequireElevation = true;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        private void Packager_OnMessage(object sender, string e)
        {
            _messages.Add(e);
            Console.WriteLine(e);
        }

        private DirectoryInfo GetModuleDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            return Directory.CreateDirectory(tempDirectory);
        }

        private FileInfo CreateScript(DirectoryInfo baseDirectory, string contents)
        {
            var tempFile = Path.Combine(baseDirectory.FullName, Guid.NewGuid().ToString()) + ".ps1";
            File.WriteAllText(tempFile, contents);
            _filesToDelete.Add(tempFile);
            return new FileInfo(tempFile);
        }

        private FileInfo CreateModule(DirectoryInfo baseDirectory, string content)
        {
            var file = Path.Combine(baseDirectory.FullName, Guid.NewGuid().ToString());
            file += ".psm1";
            File.WriteAllText(file, content);
            _filesToDelete.Add(file);
            return new FileInfo(file);
        }

        private FileInfo CreateModuleManifest(DirectoryInfo baseDirectory,
                                              FileInfo rootModule,
                                              string[] functionsToExport = null,
                                              string[] cmdletsToExport = null,
                                              string[] requiredAssemblies = null,
                                              string[] nestedModules = null)
        {
            var manifest = Path.Combine(baseDirectory.FullName, Guid.NewGuid().ToString());
            manifest += ".psd1";

            using (var powerShell = PowerShell.Create())
            {
                powerShell.AddCommand("New-ModuleManifest");
                powerShell.AddParameter("Path", manifest);
                powerShell.AddParameter("RootModule", rootModule.Name);
                powerShell.AddParameter("FunctionsToExport", functionsToExport);
                powerShell.AddParameter("CmdletsToExport", cmdletsToExport);
                powerShell.AddParameter("RequiredAssemblies", requiredAssemblies);
                powerShell.AddParameter("NestedModules", nestedModules);
                powerShell.Invoke();
            }

            _filesToDelete.Add(manifest);
            return new FileInfo(manifest);
        }

        public void Dispose()
        {
            foreach (var file in _filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {

                }
            }
        }
    }
}
