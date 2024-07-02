using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    [DataContract]
    public class PowerShellParameterSetMetadata : IPowerShellParameterSetMetadata
    {
        public PowerShellParameterSetMetadata()
        {
        }

        public PowerShellParameterSetMetadata(string name, ParameterSetMetadata metadata)
        {
            Name = name;
            HelpMessage = metadata.HelpMessage ?? string.Empty;
            IsMandatory = metadata.IsMandatory;
            Position = metadata.Position;
            ValueFromPipeline = metadata.ValueFromPipeline;
            ValueFromPipelineByPropertyName = metadata.ValueFromPipelineByPropertyName;
            ValueFromRemainingArguments = metadata.ValueFromRemainingArguments;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string HelpMessage { get; set; }

        [DataMember]
        public bool IsMandatory { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public bool ValueFromPipeline { get; set; }

        [DataMember]
        public bool ValueFromPipelineByPropertyName { get; set; }

        [DataMember]
        public bool ValueFromRemainingArguments { get; set; }
    }
}
