using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PowerShellProTools.SharedCommands
{
    [Cmdlet("Out", "PoshToolsVariable")]
    public class OutPoshToolsVariables : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSVariable InputObject { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }
        [Parameter]
        public bool ExcludeAutomatic { get; set; }

        private List<Variable> _details = new List<Variable>();

        private static string[] _automaticVariables = new[]
        {
            "$",
            "?",
            "^",
            "_",
            "args",
            "ConfirmPreference",
            "ConsoleFileName",
            "DebugPreference",
            "script:Error",
            "Error",
            "ErrorActionPreference",
            "ErrorView",
            "Event",
            "EventArgs",
            "EventSubscriber",
            "ExecutionContext",
            "false",
            "foreach",
            "FormatEnumerationLimit",
            "HOME",
            "host",
            "InformationPreference",
            "input",
            "IsCoreCLR",
            "IsLinux",
            "IsMacOs",
            "IsWindows",
            "Matches",
            "MaximumAliasCount",
            "MaximumDriveCount",
            "MaximumErrorCount",
            "MaximumFunctionCount",
            "MaximumHistoryCount",
            "MaximumVariableCount",
            "MyInvocation",
            "NestedPromptLevel",
            "null",
            "OutputEncoding",
            "PID",
            "PROFILE",
            "ProgressPreference",
            "PSBoundParameters",
            "PSCmdlet",
            "PSCommandPath",
            "PSCulture",
            "PSDefaultParameterValues",
            "PSDebugContext",
            "PSEdition",
            "PSEditor",
            "PSEmailServer",
            "PSHome",
            "PSItem",
            "PSScriptRoot",
            "PSSenderInfo",
            "PSSessionApplicationName",
            "PSSessionConfigurationName",
            "PSSessionOption",
            "PSUICulture",
            "PSVersionTable",
            "PWD",
            "Sender",
            "ShellID",
            "StackTrace",
            "switch",
            "this",
            "true",
            "VerbosePreference",
            "WarningPreference",
            "WhatIfPreference"
        };

        protected override void ProcessRecord()
        {
            try
            {
                if (InputObject.Name.Equals("PSPResults") || InputObject.Name.Equals("PSPCommand"))
                {
                    return;
                }

                if (ExcludeAutomatic && _automaticVariables.Any(m => m.Equals(InputObject.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                var value = InputObject.Value == null ? null : new PSObject(InputObject.Value);
                var variableDetails = new Variable(InputObject.Name, value);
                _details.Add(variableDetails);

                if (PassThru)
                {
                    WriteObject(variableDetails);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void EndProcessing()
        {
            GetPoshToolsVariable.VariableDetailsCache.Clear();
            GetPoshToolsVariable.VariableDetailsCache.AddRange(_details);
        }
    }
}
