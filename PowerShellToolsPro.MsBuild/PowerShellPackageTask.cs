using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PowerShellToolsPro.Cmdlets;
using PowerShellToolsPro.Packager;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PowerShellToolsPro.MsBuild
{
    public class PowerShellPackageTask : Task
    {
        public ITaskItem PackageEntryPoint { get; set; }
        public ITaskItem PackageAsExecutable { get; set; }
        public ITaskItem Bundle { get; set; }
        public ITaskItem Obfuscate { get; set; }
        public ITaskItem OutputDirectory { get; set; }
        public ITaskItem HideConsoleWindow { get; set; }
        public ITaskItem DotNetVersion { get; set; }
        public ITaskItem FileVersion { get; set; }
        public ITaskItem FileDescription { get; set; }
        public ITaskItem ProductName { get; set; }
        public ITaskItem ProductVersion { get; set; }
        public ITaskItem Copyright { get; set; }
        public ITaskItem RequireElevation { get; set; }
        public ITaskItem PackageModules { get; set; }
        public ITaskItem ApplicationIconPath { get; set; }
        public ITaskItem PackageType { get; set; }
        public ITaskItem ServiceName { get; set; }
        public ITaskItem ServiceDisplayName { get; set; }
        public ITaskItem HighDPISupport { get; set; }
        public ITaskItem PowerShellArgs { get; set; }
        public ITaskItem PackagePlatform { get; set; }
        public ITaskItem PowerShellVersion { get; set; }
        public ITaskItem OutputName { get; set; }
        public ITaskItem CompanyName { get; set; }
        public ITaskItem OperatingSystem { get; set; }
        public ITaskItem[] CompileItems { get; set; }
        public ITaskItem[] ContentItems { get; set; }
        private static bool IsNetCore => RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);

        public override bool Execute()
        {
            if (PackageEntryPoint == null || string.IsNullOrEmpty(PackageEntryPoint.ItemSpec)) return true;

            bool packageAsExecutable = false;
            if (PackageAsExecutable != null)
                bool.TryParse(PackageAsExecutable.ItemSpec, out packageAsExecutable);

            bool bundle = false;
            if (Bundle != null)
                bool.TryParse(Bundle.ItemSpec, out bundle);

            bool obfuscate = false;
            if (Obfuscate != null)
                bool.TryParse(Obfuscate.ItemSpec, out obfuscate);

            bool hideConsoleWindow = false;
            if (HideConsoleWindow != null)
                bool.TryParse(HideConsoleWindow.ItemSpec, out hideConsoleWindow);

            bool requireElevation = false;
            if (RequireElevation != null)
                bool.TryParse(RequireElevation.ItemSpec, out requireElevation);

            bool packageModules = false;
            if (PackageModules != null)
                bool.TryParse(PackageModules.ItemSpec, out packageModules);

            bool highDpiSupport = false;
            if (HighDPISupport != null)
                bool.TryParse(HighDPISupport.ItemSpec, out highDpiSupport);

            string packageType = "Console";
            if (PackageType != null)
            {
                packageType = PackageType.ItemSpec;
            }

            if (!Enum.TryParse(packageType, out Packager.Config.PackageType type))
            {
                Log.LogError($"Unknown package type: {type}");
                return false;
            }

            if (!bundle && !packageAsExecutable)
            {
                return true;
            }

            var outputDirectory = OutputDirectory.ItemSpec;

            var packageProcess = new PackageProcess();
            packageProcess.OnMessage += PackageProcess_OnMessage;
            packageProcess.OnErrorMessage += PackageProcess_OnErrorMessage;
            packageProcess.OnWarningMessage += OnWarningMessage;
            packageProcess.Bundle = bundle;
            packageProcess.PackAsExecutable = packageAsExecutable;
            packageProcess.Obfuscate = obfuscate;
            packageProcess.OutputDirectory = outputDirectory;
            packageProcess.Script = PackageEntryPoint.ItemSpec;
            packageProcess.HideConsoleWindow = hideConsoleWindow;
            packageProcess.DotNetVersion = DotNetVersion?.ItemSpec;
            packageProcess.FileVersion = FileVersion?.ItemSpec;
            packageProcess.Config.Package.CompanyName = CompanyName?.ItemSpec;
            packageProcess.Config.Package.OutputName = OutputName?.ItemSpec;
            packageProcess.Config.Package.FileDescription = FileDescription?.ItemSpec;
            packageProcess.Config.Package.ApplicationIconPath = ApplicationIconPath?.ItemSpec;
            packageProcess.Config.Package.ProductName = ProductName?.ItemSpec;
            packageProcess.Config.Package.ProductVersion = ProductVersion?.ItemSpec;
            packageProcess.Config.Package.Copyright = Copyright?.ItemSpec;
            packageProcess.Config.Package.ServiceDisplayName = ServiceDisplayName?.ItemSpec;
            packageProcess.Config.Package.ServiceName = ServiceName?.ItemSpec;
            packageProcess.Config.Package.PackageType = type;
            packageProcess.Config.Package.RequireElevation = requireElevation;
            packageProcess.Config.Bundle.Modules = packageModules;
            packageProcess.Config.Package.HighDpiSupport= highDpiSupport;
            packageProcess.Config.Package.PowerShellArguments = PowerShellArgs?.ItemSpec;
            packageProcess.Config.Package.Platform = PackagePlatform == null ? "x64" : PackagePlatform.ItemSpec;
            packageProcess.Config.Package.PowerShellVersion = PowerShellVersion == null ? "Windows PowerShell" : PowerShellVersion.ItemSpec;

            if (OperatingSystem?.ItemSpec == null)
            {
                packageProcess.Config.Package.RuntimeIdentifier = $"win-{packageProcess.Config.Package.Platform}";
            }
            else
            {
                if (OperatingSystem.ItemSpec.Equals("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    packageProcess.Config.Package.RuntimeIdentifier = $"win-{packageProcess.Config.Package.Platform}";
                }
                else if(OperatingSystem.ItemSpec.Equals("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    packageProcess.Config.Package.RuntimeIdentifier = $"linux-{packageProcess.Config.Package.Platform}";
                    if (packageProcess.Config.Package.PowerShellVersion == "Windows PowerShell")
                    {
                        throw new Exception("Linux does not support Windows PowerShell.");
                    }
                }
            }

            packageProcess.Config.Package.Resources = CompileItems == null ? new string[0] : CompileItems.Where(m => bool.TryParse(m.GetMetadata("Resource"), out bool value) && value).Select(m => m.ItemSpec).ToArray();
            if (ContentItems != null)
                packageProcess.Config.Package.Resources = ContentItems.Where(m => bool.TryParse(m.GetMetadata("Resource"), out bool value) && value).Select(m => m.ItemSpec).Concat(packageProcess.Config.Package.Resources).ToArray();

            if (packageModules)
            {
                packageProcess.Config.Bundle.Enabled = true;
            }

            var packageConfig = packageProcess.Config.Package;

            var powerShell = packageConfig.PowerShellCore ? "pwsh" : "powershell.exe";

            if (packageConfig.PowerShellCore && !IsNetCore)
            {
                powerShell = "pwsh.exe";
            }
            else if (packageConfig.Platform?.Equals("x64", StringComparison.OrdinalIgnoreCase) == true && IntPtr.Size == 4)
            {
                powerShell = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\sysnative\WindowsPowerShell\v1.0\powershell.exe");
            }

            var powerShellProcess = new Process();
            powerShellProcess.StartInfo = new ProcessStartInfo();
            powerShellProcess.StartInfo.FileName = powerShell;
            powerShellProcess.StartInfo.Arguments = "-NoExit";
            powerShellProcess.StartInfo.CreateNoWindow = true;
            powerShellProcess.StartInfo.UseShellExecute = false;

            try
            {
                powerShellProcess.Start();
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to start external PowerShell process to determine modules. " + ex.Message);
                return false;
            }

            try
            {
                var connectionInfo = new NamedPipeConnectionInfo(powerShellProcess.Id);
                using (var runspace = RunspaceFactory.CreateRunspace(connectionInfo))
                {
                    runspace.Open();

                    using (var ps = PowerShell.Create())
                    {
                        ps.Streams.Verbose.DataAdded += (s, e) => Verbose_DataAdded(e, ps.Streams.Verbose);
                        ps.Streams.Error.DataAdded += (s,e) => Error_DataAdded(e, ps.Streams.Error);
                        ps.Streams.Warning.DataAdded += (s, e) => Warning_DataAdded(e, ps.Streams.Warning);

                        var json = Serialize(packageProcess.Config);

                        ps.Runspace = runspace;
                        ps.AddCommand("Import-Module");
                        ps.AddArgument(Path.Combine(Path.GetDirectoryName(typeof(PowerShellPackageTask).Assembly.Location), "PowerShellToolsPro.Packager.dll"));
                        ps.AddStatement();
                        ps.AddCommand($"Invoke-Packager");
                        ps.AddParameter("SerializedPackageConfig", json);
                        ps.AddParameter("Verbose");
                        ps.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
                return false;
            }
            finally
            {
                powerShellProcess.Kill();
            }

            return true;
        }

        private static string Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }


        private void Warning_DataAdded(DataAddedEventArgs e, PSDataCollection<WarningRecord> collection)
        {
            Log.LogWarning(collection[e.Index].ToString());
        }

        private void Verbose_DataAdded(DataAddedEventArgs e, PSDataCollection<VerboseRecord> collection)
        {
            Log.LogMessage(MessageImportance.High, collection[e.Index].ToString());
        }

        private void Error_DataAdded(DataAddedEventArgs e, PSDataCollection<ErrorRecord> collection)
        {
            Log.LogError(collection[e.Index].ToString());
        }

        private void PackageProcess_OnErrorMessage(object sender, string e)
	    {
		    Log.LogError(e);
	    }

        private void OnWarningMessage(object sender, string e)
        {
            Log.LogWarning(e);
        }

		private void PackageProcess_OnMessage(object sender, string e)
		{
			Log.LogMessage(MessageImportance.High, e);
		}
	}
}
