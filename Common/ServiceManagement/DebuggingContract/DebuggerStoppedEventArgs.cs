using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class DebuggerStoppedEventArgs
    {
        [DataMember]
        public bool BreakpointHit { get; set; }

        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public int Line { get; set; }

        [DataMember]
        public int Column { get; set; }

        [DataMember]
        public bool OpenScript { get; set; }

        public DebuggerStoppedEventArgs()
        {
            this.BreakpointHit = false;
        }

        /// <summary>
        /// Constructor for DebuggerStoppedEventArgs
        /// </summary>
        /// <param name="script">Script which generated the stop event</param>
        /// <param name="line">Line number where the script stopped</param>
        /// <param name="column">Column number where the script stopped</param>
        /// <param name="breakpointHit">Whether or not a breakpoint hit cased the stop event, defaults to true</param>
        /// <param name="openScript">Whether or not the client should ask Visual Studio to attempt opening the script</param>
        public DebuggerStoppedEventArgs(string script, int line, int column, bool breakpointHit = true, bool openScript = false)
        {
            this.BreakpointHit = breakpointHit;
            this.ScriptFullPath = script;
            this.Line = line;
            this.Column = column;
            this.OpenScript = openScript;
        }
    }
}
