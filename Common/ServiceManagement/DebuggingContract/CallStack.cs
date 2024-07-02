using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class CallStack
    {
        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public string FunctionName { get; set; }

        [DataMember]
        public string FrameString { get; set; }

        [DataMember]
        public int StartLine { get; set; }

        [DataMember]
        public int EndLine { get; set; }

        [DataMember]
        public int StartColumn { get; set; }

        [DataMember]
        public int EndColumn { get; set; }

        public CallStack(string script, string function, int startLine)
            : this(script, function, startLine, startLine, 1, 1) // Default column set to 1 (e.g remote session debugging)
        {
        }

        public CallStack(string script, string function, int startLine, int endLine, int startColumn, int endColumn)
        {
            ScriptFullPath = script;
            FunctionName = function;
            StartLine = startLine;
            EndLine = endLine;
            StartColumn = startColumn;
            EndColumn = endColumn;
            FrameString = string.Format("at {0}, {1}: line {2}", FunctionName, ScriptFullPath, StartLine);
        }
    }
}
