using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace PowerShellToolsPro.Packager
{
    public class Obfuscator
    {
        private readonly string ObfuscarPath = Path.Combine(Path.GetTempPath(), "Obfuscar.Console.exe");

        public string Obfuscate(string assemblyPath, string destinationPath, string powerShellVersion)
        {
            if (powerShellVersion == "Windows PowerShell")
            {
                ObfuscateWinPS(assemblyPath, destinationPath);
                return destinationPath;
            }
            else
            {
                try
                {
                    CorruptPE(assemblyPath);
                }
                catch
                {
                    return assemblyPath;
                }

                return assemblyPath;
                //return ObfuscatePS(assemblyPath, destinationPath);
            }
        }

        private void CorruptPE(string assemblyPath)
        {
            var data = File.ReadAllBytes(assemblyPath);

            ReadOnlySpan<byte> bundleSignature = new byte[] {
				// 32 bytes represent the bundle signature: SHA-256 for ".net core bundle"
				0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
                0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
                0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
                0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae
            };

            for(var i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x8b && bundleSignature.SequenceEqual(new ReadOnlySpan<byte>(data, i, bundleSignature.Length)))
                {
                    data[i] = 0x00;
                }
            }

            File.WriteAllBytes(assemblyPath, data);
        }

        public void ObfuscateWinPS(string assemblyPath, string destinationPath)
        {
            ExtractObfuscar();
            var configurationFile = MakeConfigurationFile(assemblyPath);
            RunObfuscar(configurationFile);
            CopyToDestinationDirectory(assemblyPath, destinationPath);

            if (Output != null)
                Output(this, "Obfuscation complete: " + destinationPath);

            Cleanup(configurationFile, assemblyPath);
        }

        public string ObfuscatePS(string assemblyPath, string destinationPath)
        {
            var original = assemblyPath;
            assemblyPath = assemblyPath.Replace(".exe", ".dll");
            var rootPath = Path.Combine(Path.GetDirectoryName(assemblyPath), ".config");
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }

            StartDotNet("new tool-manifest", Path.GetDirectoryName(assemblyPath));
            StartDotNet("tool install --local Obfuscar.GlobalTool --version 2.2.33", Path.GetDirectoryName(assemblyPath));
            var configurationFile = MakeConfigurationFile(assemblyPath);
            StartDotNet($"tool run obfuscar.console \"{configurationFile}\"", Path.GetDirectoryName(assemblyPath));

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(assemblyPath));
            File.Delete(assemblyPath);
            File.Move(tempPath, assemblyPath);

            if (Output != null)
                Output(this, "Obfuscation complete: " + destinationPath);

            return original;
        }

        public event EventHandler<string> Output;

        private static string StartDotNet(string arguments, string workingDirectory, string exe = "dotnet")
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
                    throw new Exception("You need to install .NET Core SDK to use packaging. You can download it from Microsoft. https://dotnet.microsoft.com/download");
                }
                throw;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Failed to build.");
            }

            return output;
        }

        private void RunObfuscar(string configurationFile)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo(ObfuscarPath, "\"" + configurationFile + "\"");
            process.EnableRaisingEvents = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();
            if (Output != null)
            {
                Output(this, output);
                Output(this, errorOutput);
            }
            if (process.ExitCode != 0)
            {
                throw new Win32Exception(process.ExitCode);
            }
        }

        private void ExtractObfuscar()
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("PowerShellToolsPro.Packager.Obfuscar.Console.exe"))
            {
                using (var file = new FileStream(ObfuscarPath, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        internal static void InstallGlobalTool(string path)
        {
            StartDotNet("new tool-manifest", Path.GetDirectoryName(path));
            StartDotNet("tool install --local Obfuscar.GlobalTool --version 2.2.33", Path.GetDirectoryName(path));
        }

        internal static string MakeConfigurationFile(string assemblyPath)
        {
            var assemblyFileInfo = new FileInfo(assemblyPath);
            var tempPath = Path.GetTempPath();

            var configurationFile = new XDocument(
                new XElement("Obfuscator", 
                    new XElement("Var", new XAttribute("name", "InPath"), new XAttribute("value", assemblyFileInfo.DirectoryName)),
                    new XElement("Var", new XAttribute("name", "OutPath"), new XAttribute("value", tempPath)),
                    new XElement("Var", new XAttribute("name", "UseUnicodeNames"), new XAttribute("value", "true")),
                    new XElement("Module", new XAttribute("file", "$(InPath)\\" + assemblyFileInfo.Name))
                )
            );

            var configurationTempFile = Path.GetTempFileName();
            File.WriteAllText(configurationTempFile, configurationFile.ToString());

            return configurationTempFile;
        }

        private void Cleanup(string configurationFile, string assemblyPath)
        {
            var assemblyFileInfo = new FileInfo(assemblyPath);
            var tempPath = Path.GetTempPath();

            try
            {
                File.Delete(configurationFile);
                File.Delete(ObfuscarPath);
                File.Delete(Path.Combine(tempPath, assemblyFileInfo.Name));
            }
            catch
            {
                if (Output != null)
                    Output(this, "Failed to clean up configuration file or Obfuscar.");
            }
        }

        private void CopyToDestinationDirectory(string assemblyPath, string destinationPath)
        {
            var assemblyFileInfo = new FileInfo(assemblyPath);
            var tempPath = Path.GetTempPath();

            var fullDestPath = Path.Combine(destinationPath, assemblyFileInfo.Name);

            File.Copy(Path.Combine(tempPath, assemblyFileInfo.Name), fullDestPath, true);
        }
    }
}
