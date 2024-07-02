using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    [DebuggerDisplay("{Name}")]
    [DataContract]
    public class PowerShellModule : IPowerShellModule
    {
        public PowerShellModule()
        {
        }

        public PowerShellModule(string name)
        {
            Name = name;
        }

        public PowerShellModule(PSModuleInfo module)
        {
            Name = module.Name ?? string.Empty;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
