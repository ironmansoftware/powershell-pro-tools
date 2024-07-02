using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet("New", "PackageConfig")]
    public class NewPackageConfigCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Root { get; set; }

        [Parameter]
        public string Output { get; set; }

        [Parameter]
        public SwitchParameter Package { get; set; }
    }
}
