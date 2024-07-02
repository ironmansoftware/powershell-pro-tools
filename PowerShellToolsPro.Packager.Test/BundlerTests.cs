using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
	public class BundlerTests : IDisposable
	{
		private List<string> _scripts;
		private PsPackConfig _config;

		public BundlerTests()
		{
			_scripts = new List<string>();
			_config = new PsPackConfig
			{
				Bundle =
				{
					Enabled = true,
					Modules = true,
					NestedModules = true,
					RequiredAssemblies = true
				}
			};
		}

		[Fact]
		public void ShouldReplaceBareWordDotSourcedFiles()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript(@".\" + calledScript.Name);

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldReplaceDotSourcedJoinPathWithFiles()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript(@". (Join-Path $PSScriptRoot '" + calledScript.Name + "')");

			var bundler = new Bundler( _config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldSupportFullPaths()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript(". \"" + calledScript.FullName + "\"");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldSupportSingleQuote()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript(". '" + calledScript.FullName + "'");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldReplaceStringDotSourcedFiles()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript(". \"$PSScriptRoot\\" + calledScript.Name + "\"");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldBundleGetContent()
		{
			var calledScript = CreateScript("Write-Host Test");
			var rootScript = CreateScript("$resources = . (Join-Path $PSScriptRoot \"" + calledScript.Name + "\")");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("$resources = Write-Host Test", script.Value);
		}


		[Fact]
		public void ShouldReportWarningIfFileNotFound()
		{
			var rootScript = CreateScript(@".\someScript.ps1");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal(BundleStatus.SuccessWithWarnings, script.Status);
			Assert.Equal("Failed to resolve file name .\\someScript.ps1", script.Warnings.First());
		}

		[Fact]
		public void ShouldReplaceMultipleInstances()
		{
			var calledScript = CreateScript("Write-Host Test");

			var rootScriptContents = @".\" + calledScript.Name + Environment.NewLine;
			rootScriptContents += @".\" + calledScript.Name + Environment.NewLine;
			rootScriptContents += @".\" + calledScript.Name + Environment.NewLine;

			var rootScript = CreateScript(rootScriptContents);

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			var resultingString = "Write-Host Test" + Environment.NewLine;
			resultingString += "Write-Host Test" + Environment.NewLine;
			resultingString += "Write-Host Test" + Environment.NewLine;

			Assert.Equal(resultingString, script.Value);
		}

		[Fact]
		public void ShouldRecursivelyBundle()
		{
			var calledScript = CreateScript("Write-Host Test");
			var calledScript2 = CreateScript(". \"$PSScriptRoot\\" + calledScript.Name + "\"");
			var rootScript = CreateScript(". \"$PSScriptRoot\\" + calledScript2.Name + "\"");

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(rootScript.FullName);

			Assert.Equal("Write-Host Test", script.Value);
		}

		[Fact]
		public void ShouldBundleXaml()
		{
			var xaml = Path.Combine(Path.GetTempPath(), "MyScript.xaml");
			File.WriteAllText(xaml, "<Window></Window>");
			_scripts.Add(xaml);

			var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.xaml.ps1");
			File.WriteAllText(ps1, @"function Import-Xaml {
[xml]$xaml = Get-Content $PSScriptRoot\MyScript.xaml
}");
			_scripts.Add(ps1);

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(ps1);

			Assert.Equal("function Import-Xaml {\r\n[xml]$xaml = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('PFdpbmRvdz48L1dpbmRvdz4='))\r\n[System.Reflection.Assembly]::LoadWithPartialName('PresentationFramework') | Out-Null\r\n}", script.Value);
		}

		[Fact]
		public void ShouldBundleXamlWithDifferentName()
		{
			var xaml = Path.Combine(Path.GetTempPath(), "MyScript123.xaml");
			File.WriteAllText(xaml, "<Window></Window>");
			_scripts.Add(xaml);

			var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.xaml.ps1");
			File.WriteAllText(ps1, @"function Import-Xaml {
[xml]$xaml = Get-Content -Path $PSScriptRoot\MyScript123.xaml
}");
			_scripts.Add(ps1);

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(ps1);

			Assert.Equal("function Import-Xaml {\r\n[xml]$xaml = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('PFdpbmRvdz48L1dpbmRvdz4='))\r\n[System.Reflection.Assembly]::LoadWithPartialName('PresentationFramework') | Out-Null\r\n}", script.Value);
		}

		[Fact]
		public void ShouldBundleXamlOutsideImportXaml()
		{
			var xaml = Path.Combine(Path.GetTempPath(), "MyScript123.xaml");
			File.WriteAllText(xaml, "<Window></Window>");
			_scripts.Add(xaml);

			var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.xaml.ps1");
			File.WriteAllText(ps1, @"[xml]$xaml = Get-Content -Path $PSScriptRoot\MyScript123.xaml");
			_scripts.Add(ps1);

			var bundler = new Bundler(_config);
			var script = bundler.Bundle(ps1);

			Assert.Equal("[xml]$xaml = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('PFdpbmRvdz48L1dpbmRvdz4='))\r\n[System.Reflection.Assembly]::LoadWithPartialName('PresentationFramework') | Out-Null", script.Value);
		}


		[Fact]
        public void ShouldBundleAssemblyLoads()
        {
            var xaml = Path.Combine(Path.GetTempPath(), "Test.dll");
            File.WriteAllText(xaml, "binary");
            _scripts.Add(xaml);

            var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.ps1");
            File.WriteAllText(ps1, "[system.Reflection.Assembly]::LoadFrom(\"$PSScriptRoot\\Test.dll\")");
            _scripts.Add(ps1);

            var bundler = new Bundler(_config);
            var script = bundler.Bundle(ps1);

            Assert.Equal("[System.Reflection.Assembly]::Load([Convert]::FromBase64String('YmluYXJ5')) | Out-Null", script.Value);
        }

        [Fact]
        public void ShouldBundlAssemblyLoadWithShortName()
        {
            var xaml = Path.Combine(Path.GetTempPath(), "Test.dll");
            File.WriteAllText(xaml, "binary");
            _scripts.Add(xaml);

            var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.ps1");
            File.WriteAllText(ps1, "[Reflection.Assembly]::LoadFrom(\"$PSScriptRoot\\Test.dll\")");
            _scripts.Add(ps1);

            var bundler = new Bundler(_config);
            var script = bundler.Bundle(ps1);

            Assert.Equal("[System.Reflection.Assembly]::Load([Convert]::FromBase64String('YmluYXJ5')) | Out-Null", script.Value);
        }

        [Fact]
        public void ShouldBundlAssemblyLoadWithFullPath()
        {
            var xaml = Path.Combine(Path.GetTempPath(), "Test.dll");
            File.WriteAllText(xaml, "binary");
            _scripts.Add(xaml);

            var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.ps1");
            File.WriteAllText(ps1, $"[Reflection.Assembly]::LoadFrom('{xaml}')");
            _scripts.Add(ps1);

            var bundler = new Bundler(_config);
            var script = bundler.Bundle(ps1);

            Assert.Equal("[System.Reflection.Assembly]::Load([Convert]::FromBase64String('YmluYXJ5')) | Out-Null", script.Value);
        }

        [Fact]
        public void ShouldBundleAddType()
        {
            var xaml = Path.Combine(Path.GetTempPath(), "Test.dll");
            File.WriteAllText(xaml, "binary");
            _scripts.Add(xaml);

            var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.ps1");
            File.WriteAllText(ps1, $"Add-Type -Path (Join-Path -Path ($PSScriptRoot) -ChildPath 'Test.dll')");
            _scripts.Add(ps1);

            var bundler = new Bundler(_config);
            var script = bundler.Bundle(ps1);

            Assert.Equal("[System.Reflection.Assembly]::Load([Convert]::FromBase64String('YmluYXJ5')) | Out-Null", script.Value);
        }

        [Fact]
        public void ShouldAddWarningIfFileDoenstExist()
        {
            var ps1 = Path.Combine(Path.GetTempPath(), "MyScript.ps1");
            File.WriteAllText(ps1, $"[Reflection.Assembly]::LoadFrom('notreal.dll')");
            _scripts.Add(ps1);

            var bundler = new Bundler(_config);
            var script = bundler.Bundle(ps1);

            Assert.Equal(BundleStatus.SuccessWithWarnings, script.Status);
            Assert.Equal("File 'notreal.dll' does not exist. Bundled script may not function.", script.Warnings.First());
        }

        [Fact]
        public void ShouldBundleModules()
        {
            var rootScriptText = GetEmbeddedResource("Assets.Form.ps1");
            var calledScriptText = GetEmbeddedResource("Assets.Form.designer.ps1");

            var calledScript = CreateScript(calledScriptText);
            var rootScript = CreateScript(rootScriptText.Replace("designer.ps1", calledScript.Name));
            
            var bundler = new Bundler(_config);
            var script = bundler.Bundle(rootScript.FullName);

            ScriptBlock.Create(script.Value);
        }

        public static string GetEmbeddedResource(string resourceName)
        {
            using (Stream resourceStream = typeof(BundlerTests).Assembly.GetManifestResourceStream("PowerShellToolsPro.Packager.Test." + resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        private FileInfo CreateScript(string contents)
		{
			var tempFile = Path.GetTempFileName() + ".ps1";
			File.WriteAllText(tempFile, contents);
			_scripts.Add(tempFile);
			return new FileInfo(tempFile);
		}

		public void Dispose()
		{
			foreach (var script in _scripts)
				File.Delete(script);
		}
	}
}
