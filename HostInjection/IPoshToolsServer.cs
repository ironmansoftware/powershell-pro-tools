using PowerShellProTools.Host;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace HostInjection
{
    public interface IPoshToolsServer
    {
        IEnumerable<T> ExecutePowerShellMainRunspace<T>(string command);
        IEnumerable<Variable> GetVariables(bool excludeAutomatic);
    }

}
