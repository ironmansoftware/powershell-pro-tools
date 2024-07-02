using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.ServiceManagement
{
    [Serializable]
    public class PowerShellHostProcessException : Exception
    {
        public PowerShellHostProcessException() { }

        public PowerShellHostProcessException(string message) : base(message) { }
    }
}
