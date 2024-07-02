using System;
using System.Management.Automation;

namespace PowerShellTools.Module
{
    [Cmdlet(VerbsCommon.Get, "VSService")]
    public class GetVisualStudioServiceCommand : PoshToolsCmdlet
    {
        [Parameter(Mandatory = true)]
        public Type InterfaceType { get; set; }

        protected override void BeginProcessing()
        {
            var package = GetPackage();
            package.
        }
    }
}
