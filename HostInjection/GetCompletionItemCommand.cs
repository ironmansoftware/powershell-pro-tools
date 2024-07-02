using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro.Cmdlets.VSCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace HostInjection
{
    [Cmdlet("Get", "CompletionItem")]
    public class GetCompletionItemCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Types")]
        public SwitchParameter Types { get; set; }

        [Parameter(ParameterSetName = "Types")]
        public string IgnoredTypes { get; set; }

        [Parameter(ParameterSetName = "Types")]
        public string IgnoredAssemblies { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Commands", ValueFromPipeline = true)]
        public CommandInfo Command { get; set; }

        [Parameter(ParameterSetName = "Commands")]
        public string IgnoredModules { get; set; }

        [Parameter(ParameterSetName = "Commands")]
        public string IgnoredCommands { get; set; }

        [Parameter(ParameterSetName = "Variable", Mandatory = true, ValueFromPipeline = true)]
        public Variable Variable { get; set; }

        [Parameter(ParameterSetName = "Variable")]
        public string IgnoredVariables { get; set; }

        [Parameter(ParameterSetName = "File", Mandatory = true, ValueFromPipeline = true)]
        public FileInfo File { get; set; }

        [Parameter(ParameterSetName = "Directory", Mandatory = true, ValueFromPipeline = true)]
        public DirectoryInfo Directory { get; set; }

        private IEnumerable<Regex> ignoredModules = new Regex[0];
        private IEnumerable<Regex> ignoredCommands = new Regex[0];
        private IEnumerable<Regex> ignoredAssemblies = new Regex[0];
        private IEnumerable<Regex> ignoredTypes = new Regex[0];
        private IEnumerable<Regex> ignoredVariables = new Regex[0];

        protected override void BeginProcessing()
        {
            if (!string.IsNullOrEmpty(IgnoredModules))
                ignoredModules = IgnoredModules.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(IgnoredCommands))
                ignoredCommands = IgnoredCommands.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(IgnoredAssemblies))
                ignoredAssemblies = IgnoredAssemblies.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(IgnoredTypes))
                ignoredTypes = IgnoredTypes.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(IgnoredVariables))
                ignoredVariables = IgnoredVariables.Split(';').Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToArray();
        }

        protected override void ProcessRecord()
        {
            if (Variable != null && Variable.VarName != null)
            {
                if (ignoredVariables.Any(m => m.IsMatch(Variable.VarName)))
                {
                    return;
                }

                WriteObject(new CompletionItem(Variable));
            }

            if (Command != null)
            {
                if (ignoredModules.Any(m => m.IsMatch(Command.ModuleName)))
                {
                    return;
                }

                if (ignoredCommands.Any(m => m.IsMatch(Command.Name)))
                {
                    return;
                }

                WriteObject(new CompletionItem(Command));
            }

            if (File != null)
            {
                WriteObject(new CompletionItem(File));
            }

            if (Directory != null)
            {
                WriteObject(new CompletionItem(Directory));
            }
        }

        protected override void EndProcessing()
        {
            if (Types)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (ignoredAssemblies.Any(m => m.IsMatch(assembly.FullName)))
                    {
                        continue;
                    }

                    foreach(var type in assembly.GetTypes())
                    {
                        if (ignoredTypes.Any(m => m.IsMatch(type.FullName)))
                        {
                            continue;
                        }

                        WriteObject(new CompletionItem(type));
                    }
                }
            }
        }
    }
}
