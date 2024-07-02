using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace HostInjection.Models
{
    public class PSHostProcess
    {
        public PSHostProcess(PSObject psobject)
        {
            Process = psobject.Properties["ProcessName"].Value.ToString();
            ProcessId = (int)psobject.Properties["ProcessId"].Value;
        }
        public string Process { get; set; }
        public int ProcessId { get; set; }
    }
}
