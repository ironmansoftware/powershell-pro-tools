using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell;
using System.Runtime.InteropServices;
using System;
using System.ComponentModel;
using System.Text;
using System.IO.Compression;
using System.ServiceProcess;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Configuration.Install;


namespace PowerShellToolsPro.Packager.ConsoleHost
{
	class Program
	{
        static void Main(string[] args)
		{
            string modulePath = string.Empty;
            try
            {
                if (System.Environment.UserInteractive)
                {
                    string parameter = string.Concat(args);
                    switch (parameter)
                    {
                        case "--install":
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--uninstall":
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            break;
                    }
                }
                else
                {
                    var moduleZip = Path.GetTempFileName();
                    modulePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    if (WriteResourceToFile("//FileName.Modules.zip", moduleZip))
                    {
                        ZipFile.ExtractToDirectory(moduleZip, modulePath);
                        AddValueToPathEnvVar(modulePath);
                    }

                    String script;
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("//FileName.script.ps1"))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            script = reader.ReadToEnd();
                        }
                    }

#if OBFUSCATE
            script = Decrypt(script);
#endif

                    var contents = ReplaceString(script, "$PSScriptRoot", "$PoshToolsRoot", StringComparison.OrdinalIgnoreCase);

                    var runspace = RunspaceFactory.CreateRunspace();
                    runspace.Open();

                    using (var powerShell = PowerShell.Create())
                    {
                        powerShell.Runspace = runspace;
                        powerShell.AddCommand("Set-Variable").AddParameter("Name", "ProcessArgs").AddParameter("Value", args);
                        powerShell.Invoke();
                    }

                    using (var powerShell = PowerShell.Create())
                    {
                        powerShell.Runspace = runspace;
                        powerShell.AddScript(contents);
                        powerShell.Invoke();
                    }

                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new Service(runspace)
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(modulePath))
                    DeleteModuleDirectory(modulePath);
            }
        }

#if OBFUSCATE
        private static string Decrypt(string cipherString)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            string key = "//key";
            keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
#endif

        public static bool WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null) return false;

                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }

                return true;
            }
        }

        public static void AddValueToPathEnvVar(string path)
        {
            var pathvar = Environment.GetEnvironmentVariable("PSModulePath");
            pathvar += ";" + path;
            Environment.SetEnvironmentVariable("PSModulePath", pathvar);
        }

        private static void DeleteModuleDirectory(string directory)
        {
            var powershell = new Process();
            powershell.StartInfo = new ProcessStartInfo();
            powershell.StartInfo.FileName = "powershell";
            powershell.StartInfo.Arguments = $"-WindowStyle Hidden -NoProfile -NonInteractive -Command \"Start-Sleep 2; Remove-Item '{directory}' -Force -Recurse\"";
            powershell.Start();
        }

        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }

    public class Service : ServiceBase
    {
        private System.ComponentModel.IContainer components = null;
        private Runspace runspace;
        private string serviceName;

        public Service(Runspace runspace)
        {
            this.runspace = runspace;
            serviceName = "//ServiceName";

            try
            {
                if (!EventLog.SourceExists(serviceName))
                {
                    EventLog.CreateEventSource(serviceName, "Application");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create event source: " + ex.Message);
            }

            components = new System.ComponentModel.Container();
            this.ServiceName = serviceName;

            var canStop = runspace.SessionStateProxy.GetVariable("CanStop");
            if (canStop != null)
            {
                this.CanStop = (bool)canStop;
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", "Service").AddParameter("Value", this);
                    powerShell.Invoke();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", "PoshToolsRoot").AddParameter("Value", AssemblyDirectory);
                    powerShell.Invoke();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", "ServiceArgs").AddParameter("Value", args);
                    powerShell.Invoke();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("OnStart");
                    powerShell.Invoke();

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            EventLog eventLog = new EventLog();
                            eventLog.Source = serviceName;
                            eventLog.WriteEntry($"{error.ToString()} {Environment.NewLine} {error.ScriptStackTrace}", EventLogEntryType.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog eventLog = new EventLog();
                eventLog.Source = serviceName;
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", "Service").AddParameter("Value", this);
                    powerShell.Invoke();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", "PoshToolsRoot").AddParameter("Value", AssemblyDirectory);
                    powerShell.Invoke();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddCommand("OnStop").AddArgument(this);
                    powerShell.Invoke();

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            EventLog eventLog = new EventLog();
                            eventLog.Source = serviceName;
                            eventLog.WriteEntry($"{error.ToString()} {Environment.NewLine} {error.ScriptStackTrace}", EventLogEntryType.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog eventLog = new EventLog();
                eventLog.Source = serviceName;
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    [RunInstaller(true)]
    public class MyWindowsServiceInstaller : Installer
    {
        public MyWindowsServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = "//ServiceDisplayName";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.ServiceName = "//ServiceName";
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}