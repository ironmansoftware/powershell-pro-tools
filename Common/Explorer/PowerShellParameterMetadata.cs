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
    [DebuggerDisplay("{Name}, Type {Type}")]
    [DataContract]
    public class PowerShellParameterMetadata : IPowerShellParameterMetadata
    {
        public PowerShellParameterMetadata()
        {
        }

        public PowerShellParameterMetadata(string name, ParameterMetadata commandMetadata)
        {
            Name = name;
            IsDynamic = commandMetadata.IsDynamic;
            SwitchParameter = commandMetadata.SwitchParameter;
            Type = ConversionFactory.MapParameterType(commandMetadata.ParameterType);
            ParameterSets = ConversionFactory.Convert(commandMetadata.ParameterSets);
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public bool IsDynamic 
        { 
            get; 
            set; 
        }

        [DataMember]
        public bool SwitchParameter 
        { 
            get; 
            set; 
        }

        [DataMember]
        public ParameterType Type 
        { 
            get; 
            set; 
        }

        [DataMember]
        public List<IPowerShellParameterSetMetadata> ParameterSets 
        { 
            get; 
            set; 
        }
    }
}
