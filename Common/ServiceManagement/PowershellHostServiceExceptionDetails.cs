using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement
{
    [DataContract]
    public sealed class PowerShellHostServiceExceptionDetails
    {
        public static readonly PowerShellHostServiceExceptionDetails Default = new PowerShellHostServiceExceptionDetails();

        [DataMember]
        public String Message { get; private set; }

        public PowerShellHostServiceExceptionDetails()
        {
            this.Message = "There is a problem in the PowerShell host service.";
        }

        public PowerShellHostServiceExceptionDetails(String message)
        {
            this.Message = message;
        }
    }
}
