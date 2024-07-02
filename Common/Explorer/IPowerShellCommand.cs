using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public interface IPowerShellCommand
    {
        string Name { get; set; }
        string ModuleName { get; set; }
        string Definition { get; set; }
        CommandTypes Type { get; set; }
        bool SupportsCommonParameters { get; set; }
    }
}
