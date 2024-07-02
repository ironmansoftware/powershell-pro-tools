using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public interface IPowerShellCommandMetadata
    {
        string Name { get; set; }
        List<IPowerShellParameterMetadata> Parameters { get; set; }
    }
}
