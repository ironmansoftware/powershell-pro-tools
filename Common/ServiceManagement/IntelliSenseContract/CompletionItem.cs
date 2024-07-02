using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    [DataContract]
    public sealed class CompletionItem
    {
        public CompletionItem(string completionText, string listItemText, int resultType, string toolTip)
        {
            CompletionText = completionText;
            ListItemText = listItemText;
            ResultType = resultType;
            ToolTip = toolTip;
        }

        [DataMember]
        public string CompletionText { get; private set; }

        [DataMember]
        public string ListItemText { get; private set; }

        [DataMember]
        public int ResultType { get; private set; }

        [DataMember]
        public string ToolTip { get; private set; }
    }
}
