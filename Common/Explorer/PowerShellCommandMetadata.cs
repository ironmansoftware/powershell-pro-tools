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
    //[DebuggerDisplay]
    [DataContract]
    public class PowerShellCommandMetadata : IPowerShellCommandMetadata
    {
        public PowerShellCommandMetadata()
        {
        }

        public PowerShellCommandMetadata(CommandMetadata commandMetadata)
        {
            Name = commandMetadata.Name ?? string.Empty;
            Parameters = ConversionFactory.Convert(commandMetadata.Parameters);
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public List<IPowerShellParameterMetadata> Parameters
        {
            get;
            set;
        }
    }
}
