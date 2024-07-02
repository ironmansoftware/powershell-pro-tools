using PowerShellTools.Common.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class PowerShellRunTerminatingException
    {
        [DataMember]
        public string Message;

        [DataMember]
        public ErrorRecord Error;

        public PowerShellRunTerminatingException(Exception ex)
        {
            if (ex is IContainsErrorRecord)
            {
                Error = ((IContainsErrorRecord)ex).ErrorRecord;
                Message = string.Format(DebugEngineConstants.TerminatingErrorFormat,
                    ex.Message,
                    Environment.NewLine,
                    Error.CategoryInfo,
                    Error.FullyQualifiedErrorId);
            }
            else
            {
                Error = null;
                Message = ex.Message;
            }
        }
    }
}
