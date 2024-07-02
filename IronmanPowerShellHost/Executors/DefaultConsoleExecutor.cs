using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace IronmanPowerShellHost.Executors
{
    internal class DefaultConsoleExecutor : IExecutor
    {
        public int Run(string script, string[] args)
        {
            var arguments = new List<string>();
            arguments.Add("-Command");
            arguments.Add(script.TrimEnd(new[] { '\r', '\n' }));
            arguments.AddRange(args);

#if NET472
            var config = RunspaceConfiguration.Create();
            config.InitializationScripts.Append(new ScriptConfigurationEntry("Init", $"$Global:ExecutableRoot = '{AssemblyDirectory}'"));
            return ConsoleShell.Start(config, null, null, arguments.ToArray());
#else
            return ConsoleShell.Start(InitialSessionState.CreateDefault(), null, null, arguments.ToArray());
#endif
        }

        public static string AssemblyDirectory
        {
            get
            {
#pragma warning disable SYSLIB0012
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
#pragma warning restore SYSLIB0012
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
