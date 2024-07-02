using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
    public class OldCompilerTests : IDisposable
    {
        private List<string> _filesToDelete = new List<string>();
        private List<string> _dirsToDelete = new List<string>();
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

        [Fact(Skip = "Skip")]
        public void ShouldSignAssembly()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $"Write-Host 'Hello'");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.Certificate = @"Cert:\CurrentUser\My\73C86B5EDAB13B83FEDEFC59BE4D2920E66CA486";
            //config.Package.Certificate = @"Cert:\LocalMachine\Root\A862D64A0CB39AF0B406468108EE83639F40A055";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }

        [Fact(Skip = "Requires customer module")]
        public void ShouldBundleCustomerProject()
        {
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(@"C:\Users\adamr\Desktop\test\CheckIfStudentExistsForCampusRes.form.ps1", outputDir);

            config.Bundle.RequiredAssemblies = false;
            config.Bundle.NestedModules = false;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }

        [Fact(Skip = "Requires module")]
        public void ShouldBundldNTFSSecurity()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $"Import-Module NTFSSecurity; Get-NTFSAccess -Path C:\\src");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }

        [Fact(Skip = "Requires module")]
        public void ShouldBundleLiteDbModule()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), @"
import-module pslitedb

New-LiteDBDatabase -Path ./test.db
Open-LiteDBConnection -Database ./test.db

New-LiteDBCollection -Collection SvcCollection

Get-Service b* |
  select @{Name = '_id';E={$_.Name}},DisplayName,Status,StartType |
      ConvertTo-LiteDbBSON |
         Add-LiteDBDocument -Collection SvcCollection

");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }

        [Fact(Skip = "Requires AD")]
        public void ShouldBundleADModule()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $"Write-Host 'Hello'; Import-Module ActiveDirectory; Get-ADUser -Identity *");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.0.0";
            config.Package.DotNetVersion = "net462";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }


        [Fact(Skip = "Requires module")]
        public void ShouldBundleMSAL()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $"Write-Host 'Hello'; Import-Module MSAL;");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.0.0";
            config.Package.DotNetVersion = "netcoreapp31";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }


        [Fact(Skip = "Requires module")]
        public void ShouldBundlePowerCLI()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $"Write-Host 'Hello'; Import-Module VMWare.PowerCLI;");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            //config.Package.PowerShellVersion = "7.0.0";
            //config.Package.DotNetVersion = "netcoreapp31";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);
        }

        [Theory(Skip = "Skip")]
        [InlineData("7.0.0", "netcoreapp31", "linux-x64")]
        [InlineData("7.1.0", "net5.0", "linux-x64")]
        [InlineData("7.2.0", "net6.0", "linux-x64")]
        [InlineData("7.2.4", "net6.0", "linux-x64")]
        [InlineData("7.2.5", "net6.0", "linux-x64")]
        [InlineData("7.2.6", "net6.0", "linux-x64")]
        [InlineData("7.0.0", "netcoreapp31", "win-x64")]
        [InlineData("7.1.0", "net5.0", "win-x64")]
        [InlineData("7.2.0", "net6.0", "win-x64")]
        [InlineData("7.2.4", "net6.0", "win-x64")]
        [InlineData("7.2.5", "net6.0", "win-x64")]
        [InlineData("7.2.6", "net6.0", "win-x64")]
        public void ShouldWorkWithCore(string psVerion, string dotnetVersion, string runtime)
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ Start }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"'Hello, Linux!' | Out-File '.\\test2.txt'");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = psVerion;
            config.Package.DotNetVersion = dotnetVersion;
            config.Package.RuntimeIdentifier = runtime;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);

            Assert.True(v.Success);

            module.Delete();
        }


        [Fact(Skip = "Skip")]
        public void ShouldWorkWithIcon()
        {

            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"$args.Length | Out-File '{tempFile}' ");

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PowerShellToolsPro.Packager.Test.Assets.icon.ico";

            var icon = Path.GetTempFileName();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new BinaryReader(stream))
            {
                var result = reader.ReadBytes((int)stream.Length);
                File.WriteAllBytes(icon, result);
            }

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);
            config.Package.PowerShellVersion = "7.2.0";
            config.Package.DotNetVersion = "net6.0";
            config.Package.RuntimeIdentifier = "win-x64";
            config.Package.ApplicationIconPath = icon;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.OnMessage += OnMessage;
            packager.OnErrorMessage += OnMessage;
            packager.OnWarningMessage += OnMessage;

            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact()]
        public void ShouldWorkWith72AndArgs()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"$args.Length | Out-File '{tempFile}' ");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.2.0";
            config.Package.DotNetVersion = "net6.0";
            config.Package.RuntimeIdentifier = "win-x64";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.OnMessage += OnMessage;
            packager.OnErrorMessage += OnMessage;
            packager.OnWarningMessage += OnMessage;

            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe, "nice cool awesome");
            p.WaitForExit();

            _filesToDelete.Add(exe);
            _dirsToDelete.Add(outputDir);

            var retry = 0;
            while (retry < 30)
            {
                if (File.Exists(tempFile) && !string.IsNullOrEmpty(File.ReadAllText(tempFile)))
                {
                    break;
                }
                Thread.Sleep(1000);
                retry++;
            }

            Assert.Equal("3\r\n", File.ReadAllText(tempFile));
        }


        [Fact()]
        public void ShouldWorkWith71AndArgs()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"$args.Length | Out-File '{tempFile}' ");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.1.4";
            config.Package.DotNetVersion = "net5.0";
            config.Package.RuntimeIdentifier = "win-x64";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.OnMessage += OnMessage;
            packager.OnErrorMessage += OnMessage;
            packager.OnWarningMessage += OnMessage;

            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe, "nice cool awesome");
            p.WaitForExit();

            _filesToDelete.Add(exe);

            var retry = 0;
            while (retry < 30)
            {
                if (File.Exists(tempFile) && !string.IsNullOrEmpty(File.ReadAllText(tempFile)))
                {
                    break;
                }
                Thread.Sleep(1000);
                retry++;
            }

            Assert.Equal("3\r\n", File.ReadAllText(tempFile));
        }

        private void OnMessage(object sender, string e)
        {
            Console.WriteLine(e);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithCoreAndArgs()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"$args.Length | Out-File '{tempFile}' ");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.0.0";
            config.Package.DotNetVersion = "netcoreapp31";
            config.Package.RuntimeIdentifier = "win-x64";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe, "nice cool awesome");
            p.WaitForExit();

            _filesToDelete.Add(exe);

            Assert.Equal("3\r\n", File.ReadAllText(tempFile));
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithCoreWin()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module '{module.FullName}'\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.PowerShellVersion = "7.0.0";
            config.Package.DotNetVersion = "netcoreapp31";
            config.Package.RuntimeIdentifier = "win-x64";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            _filesToDelete.Add(exe);

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.Equal("Succeeded\r\n", File.ReadAllText(tempFile));
        }

        [Fact(Skip = "Skip")]
        public void ShouldUseAlternateSdk()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $" using namespace System.Windows.Forms\r\nImport-Module '{module.FullName}'\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            config.Package.DotNetSdk = "3.1.411";
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe);
            p.WaitForExit();

            _filesToDelete.Add(exe);

            Assert.Equal("Succeeded\r\n", File.ReadAllText(tempFile));
        }


        [Fact(Skip = "Skip")]
        public void ShouldWOrkWithUsings()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $" using namespace System.Windows.Forms\r\nImport-Module '{module.FullName}'\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe);
            p.WaitForExit();

            _filesToDelete.Add(exe);

            Assert.Equal("Succeeded\r\n", File.ReadAllText(tempFile));
        }


        [Fact(Skip = "Skip")]
        public void ShouldRequireElevation()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.RequireElevation = true;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.OnErrorMessage += Packager_OnErrorMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        private void Packager_OnErrorMessage(object sender, string e)
        {
            Console.WriteLine(e);
        }

        [Fact(Skip = "Skip")]
        public void ShouldIgnoreModule()
        {
            var tempFile = Path.GetTempFileName();
            var moduleContent = $"function MyFunction {{ 'Succeeded' | Out-File '{tempFile}' }}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $"try {{  Import-Module -Name {module.FullName};MyFunction }} catch {{ 'Failed' | Out-File '{tempFile}' }} ");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Bundle.IgnoredModules = new[] { module.Name.Replace(".psm1", string.Empty) };
            var v = packager.Execute();

            Assert.True(v.Success);

            module.Delete();

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.Equal("Failed\r\n", File.ReadAllText(tempFile));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldObfuscate()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var tempFile = Path.GetTempFileName();
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\n");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);
            config.Package.Obfuscate = true;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldObfuscatePS72()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var tempFile = Path.GetTempFileName();
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\n");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);


            config.Package.PowerShellVersion = "7.2.0";
            config.Package.DotNetVersion = "net6.0";
            config.Package.RuntimeIdentifier = "win-x64";
            config.Package.Obfuscate = true;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldObfuscatePS71()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var tempFile = Path.GetTempFileName();
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\n");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);


            config.Package.PowerShellVersion = "7.1.4";
            config.Package.DotNetVersion = "net5.0";
            config.Package.RuntimeIdentifier = "win-x64";
            config.Package.Obfuscate = true;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldCompileScriptModule()
        {
            var moduleContent = "function MyFunction{write-Host 'hey'}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var tempFile = Path.GetTempFileName();
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\n");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithProcessObject()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), @"
$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = 'bcdedit.exe'
$pinfo.RedirectStandardError = $true
$pinfo.RedirectStandardOutput = $true
$pinfo.UseShellExecute = $false
$pinfo.Arguments = '/enum'
$p = New-Object System.Diagnostics.Process
$p.StartInfo = $pinfo
$p.Start() | Out-Null
$p.WaitForExit()
$p.StandardOutput.ReadToEnd()
");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = new Process();
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = exe;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();

            var s = p.StandardOutput.ReadToEnd();

            _filesToDelete.Add(exe);

            //Assert.True(s == "something");
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithPSScriptRoot()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "'Hello' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithEnvironmentVariables()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $". \"$Env:Temp\\$test.txt\"");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithPSScriptRootFromWorkingFolderWithSpace()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "'Hello' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + " " + "hasaspace");
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var pathwithSpaces = Path.Combine(Path.GetTempPath(), "Path with spaces");
            if (!Directory.Exists(pathwithSpaces))
            {
                Directory.CreateDirectory(pathwithSpaces);
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = exe;
            startInfo.WorkingDirectory = pathwithSpaces;

            var p = Process.Start(startInfo);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldCreateCoreService()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "function OnStart() { 'Hello' | Out-File 'C:\\users\\adamr\\nice.txt' }");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.ServiceName = "PoshServ";

            var packager = new PackageProcess();
            config.Package.PackageType = PackageType.Service;
            config.Package.DotNetVersion = "netcoreapp3.1";
            config.Package.PowerShellVersion = "7.0.3";
            config.Package.RuntimeIdentifier = "win10-x64";
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldCreatePs72Service()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "function OnStart() { 'Hello' | Out-File 'C:\\users\\adamr\\nice.txt' }");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.ServiceName = "PoshServ";

            var packager = new PackageProcess();
            config.Package.PackageType = PackageType.Service;
            config.Package.DotNetVersion = "net6.0";
            config.Package.PowerShellVersion = "7.2.0";
            config.Package.RuntimeIdentifier = "win10-x64";
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);


        }

        [Fact(Skip = "Skip")]
        public void ShouldCreateService()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "function OnStart() { 'Hello' | Out-File 'C:\\users\\adamr\\nice.txt' }");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            config.Package.ServiceName = "PoshServ";

            var packager = new PackageProcess();
            config.Package.PackageType = PackageType.Service;
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithEncoding()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "'åäö' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("åäö\r\n", contents);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldTakeParameters()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "param([Switch]$Help) $Help.IsPresent | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe, "-help");
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("True\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldDisableQuickEdit()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "Write-Host 'Hi'");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            packager.Config.Package.DisableQuickEdit = true;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithApostropheInOutputPath()
        {
            new DirectoryInfo(Path.GetTempPath()).Create();
            var script = CreateScript(new DirectoryInfo(Path.GetTempPath()), "'Hi' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), $". \"{script}\"");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "'s");
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("Hi\r\n", contents);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldWorkWithApostropheInPath()
        {
            new DirectoryInfo(Path.GetTempPath() + "Script's").Create();
            var script = CreateScript(new DirectoryInfo(Path.GetTempPath() + "Script's"), "'Hi' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath() + "Script's"), $". \"{script}\"");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("Hi\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldRunInPathWithApostophe()
        {
            var dir = new DirectoryInfo(Path.GetTempPath() + "Script's");
            dir.Create();
            var script = CreateScript(new DirectoryInfo(Path.GetTempPath() + "Script's"), "'Hi' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath() + "Script's"), $". \"{script}\"");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            var newExe = Path.Combine(Path.GetTempPath(), "Script's", "my.exe");
            File.Copy(exe, newExe, true);

            Assert.True(File.Exists(newExe));

            var p = Process.Start(newExe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(dir.FullName, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(dir.FullName, "test.txt"));
            Assert.Equal("Hi\r\n", contents);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldWorkWithCmdletBinding()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[CmdletBinding()]\r\nparam() 'Hi' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("Hi\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithArgs()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[CmdletBinding()]\r\nparam($Arg) $Arg | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe, "-Arg 'Hello'");
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("Hello\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithArgsSwitch()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[CmdletBinding()]\r\nparam([Switch]$Arg) $Arg.IsPresent | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe, "-Arg");
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("True\r\n", contents);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldWorkWithArgsWithSpaces()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[CmdletBinding()]\r\nparam($Arg) $Arg | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe, "-Arg \"Hello world!\"");
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("Hello world!\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithArrays()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "param([string[]]$Strings) $Strings | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe, "@('test', 'test', 'test')");
            p.WaitForExit();

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt"));
            Assert.Equal("test\r\ntest\r\ntest\r\n", contents);

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldTakePowerShellArgs()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);
            config.Package.PowerShellArguments = "-NoExit";

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit(2000);

            try
            {
                Assert.False(p.HasExited);
            }
            finally
            {
                p.Kill();
            }

            _filesToDelete.Add(exe);
        }

        [Theory(Skip = "False")]
        [InlineData("x86", "4")]
        [InlineData("x64", "8")]
        public void ShouldCompilePlatforms(string platform, string output)
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "[IntPtr]::Size | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);
            config.Package.Platform = platform;

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Thread.Sleep(1000);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit(2000);

            Thread.Sleep(2000);

            Assert.True(File.Exists(Path.Combine(outputDir, "test.txt")));

            var contents = File.ReadAllText(Path.Combine(outputDir, "test.txt")).Trim();
            Assert.Equal(output, contents);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldSetFileProperties()
        {
            var rootScript = CreateScript(new DirectoryInfo(Path.GetTempPath()), "'Hello' | Out-File (Join-Path $PSScriptRoot 'test.txt')");
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new PsPackConfig
            {
                OutputPath = outputDir,
                Root = rootScript.FullName,
                Bundle =
                {
                    Enabled = true,
                    Modules = true,
                    NestedModules = true,
                    RequiredAssemblies = true
                },
                Package = new PackageConfig
                {
                    Enabled = true,
                    FileDescription = "Description",
                    FileVersion = "9.0.0.0",
                    Copyright = "Copyright",
                    ProductVersion = "10.0.0.0",
                    ProductName = "Product"
                }
            };

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var info = FileVersionInfo.GetVersionInfo(exe);
            Assert.Equal("9.0.0.0", info.FileVersion);
            Assert.Equal("Copyright", info.LegalCopyright);
            Assert.Equal("10.0.0.0", info.ProductVersion);
            Assert.Equal("Product", info.ProductName);

            _filesToDelete.Add(exe);
        }


        [Fact(Skip = "Skip")]
        public void ShouldRunFunctionFromModuleInFolder()
        {
            var tempFileName = Path.GetTempFileName();

            var scriptContent = $"'Woo' | Out-File {tempFileName}";

            var rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(rootDir);
            var scriptDirName = Guid.NewGuid().ToString();
            var scriptDir = Path.Combine(rootDir, scriptDirName);
            Directory.CreateDirectory(scriptDir);

            var nestedScript = CreateScript(new DirectoryInfo(scriptDir), scriptContent);
            var rootScript = CreateScript(new DirectoryInfo(rootDir), $". (Join-Path $PSScriptRoot\\{scriptDirName} '{nestedScript.Name}')");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldBundleDotSourceModule()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name .\\{manifest.Name}\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldntFailWithFunctionScope()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Function:\\Invoke-PopulateUserTable");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldRunFunctionFromModule()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\nMyFunction");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldBundleMultipleModules()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var module1 = CreateModule(moduleDirectory, moduleContent);
            var manifest1 = CreateModuleManifest(moduleDirectory, module);
            var module2 = CreateModule(moduleDirectory, moduleContent);
            var manifest2 = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\nImport-Module -Name {manifest1.FullName}\r\nImport-Module -Name {manifest2.FullName}\r\n");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);

        }

        [Fact(Skip = "Skip")]
        public void ShouldWorkWithSpaceInScriptName()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);
            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName}\r\nMyFunction");
            rootScript.MoveTo(rootScript.FullName.Replace(".ps1", " .ps1"));

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldBundleResources()
        {
            var tempFileName = Path.GetTempFileName();

            var moduleContent = $"'Woo' | Out-File {tempFileName}";
            var moduleDirectory = GetModuleDirectory();
            var module = CreateScript(moduleDirectory, moduleContent);
            var rootScript = CreateScript(moduleDirectory, $". (Join-Path $PSScriptRoot '{module.Name}')");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));

            Assert.True(File.Exists(exe));

            var p = Process.Start(exe);
            p.WaitForExit(2000);

            Assert.True(File.Exists(tempFileName));
            Assert.Equal("Woo\r\n", File.ReadAllText(tempFileName));

            _filesToDelete.Add(exe);
        }

        [Fact(Skip = "Skip")]
        public void ShouldPackageTwoModules()
        {
            var tempFileName = Path.GetTempFileName();
            var tempFileName2 = Path.GetTempFileName();

            var moduleContent = $"function MyFunction{{'Woo' | Out-File {tempFileName}}}";
            var moduleContent2 = $"function MyFunction2{{'Woo' | Out-File {tempFileName}}}";

            var moduleDirectory = GetModuleDirectory();
            var module = CreateModule(moduleDirectory, moduleContent);
            var manifest = CreateModuleManifest(moduleDirectory, module);

            var module2 = CreateModule(moduleDirectory, moduleContent2);
            var manifest2 = CreateModuleManifest(moduleDirectory, module2);

            var rootScript = CreateScript(moduleDirectory, $"Import-Module -Name {manifest.FullName};Import-Module -Name {manifest.FullName}\r\nMyFunction;MyFunction2");

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = MakeConfig(rootScript.FullName, outputDir);

            var packager = new PackageProcess();
            packager.OnMessage += Packager_OnMessage;
            packager.Config = config;
            var v = packager.Execute();

            Assert.True(v.Success);

            Assert.True(File.Exists(tempFileName));
            Assert.True(File.Exists(tempFileName2));

            var exe = Path.Combine(config.OutputPath, rootScript.Name.Replace(".ps1", ".exe"));
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

            foreach(var dir in _dirsToDelete)
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {

                }
            }
        }
    }
}
