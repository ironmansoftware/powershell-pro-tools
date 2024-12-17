using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PowerShellProTools.SharedCommands
{
    [Cmdlet("Get", "PoshToolsVariable")]
    public class GetPoshToolsVariable : PSCmdlet
    {
        public static List<Variable> VariableDetailsCache = new List<Variable>();

        [Parameter(Mandatory = true)]
        public string Path { get; set; }

        [Parameter()]
        public SwitchParameter ValueOnly { get; set; }

        protected override void ProcessRecord()
        {
            var variables = VariableDetailsCache.Where(m => Path.StartsWith(m.Path));

            foreach (var variable in variables)
            {
                if (variable.Path.Equals(Path, StringComparison.OrdinalIgnoreCase))
                {
                    if (ValueOnly)
                    {
                        WriteObject(variable.Value);
                    }
                    else
                    {
                        WriteObject(variable.GetChildren(), true);
                    }
                }

                var child = variable.FindChild(Path);
                if (child != null)
                {
                    if (ValueOnly)
                    {
                        WriteObject(child.Value);
                    }
                    else
                    {
                        WriteObject(child.GetChildren(), true);
                    }
                }
            }
        }
    }

}
