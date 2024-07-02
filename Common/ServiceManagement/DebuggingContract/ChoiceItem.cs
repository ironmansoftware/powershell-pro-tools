using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Host;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public sealed class ChoiceItem
    {
        public ChoiceItem(ChoiceDescription choiceDescription)
        {
            if (choiceDescription == null)
            {
                throw new ArgumentNullException("choiceDescription");
            }

            this.Label = choiceDescription.Label.Replace('&', '_');
            this.HelpMessage = choiceDescription.HelpMessage;
        }

        public ChoiceItem(string label, string message)
        {
            this.Label = label;
            this.HelpMessage = message;
        }
        
        [DataMember]
        public string Label
        {
            get;
            private set;
        }

        [DataMember]
        public string HelpMessage
        {
            get;
            private set;
        }
    }
}
