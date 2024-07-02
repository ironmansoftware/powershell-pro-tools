using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public interface IPowerShellParameterSetMetadata
    {
        string Name { get; set; }
        string HelpMessage { get; set; }
        bool IsMandatory { get; set; }
        int Position { get; set; }
        bool ValueFromPipeline { get; set; }
        bool ValueFromPipelineByPropertyName { get; set; }
        bool ValueFromRemainingArguments { get; set; }
    }
}
