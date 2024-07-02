using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public interface IPowerShellParameterMetadata
    {
        string Name { get; set; }
        bool IsDynamic { get; set; }
        bool SwitchParameter { get; set; }
        ParameterType Type { get; set; }
        List<IPowerShellParameterSetMetadata> ParameterSets { get; set; }
    }
}
