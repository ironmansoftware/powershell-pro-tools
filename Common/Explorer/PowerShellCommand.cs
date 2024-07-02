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
    [DebuggerDisplay("{Name}, Module {ModuleName}")]
    [DataContract]
    public class PowerShellCommand : IPowerShellCommand
    {
        public PowerShellCommand()
        {
        }

        public PowerShellCommand(CommandInfo command)
        {
            Name = command.Name;
            ModuleName = command.ModuleName ?? string.Empty;
            Definition = command.Definition ?? string.Empty;
            Type = command.CommandType;
            SupportsCommonParameters = 
                (Type & CommandTypes.Cmdlet) == CommandTypes.Cmdlet | 
                (Type & CommandTypes.Function) == CommandTypes.Function;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public string ModuleName
        {
            get;
            set;
        }

        [DataMember]
        public string Definition 
        { 
            get; 
            set;
        }

        [DataMember]
        public CommandTypes Type
        {
            get;
            set;
        }

        [DataMember]
        public bool SupportsCommonParameters
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
