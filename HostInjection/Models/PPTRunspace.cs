using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace HostInjection.Models
{
    public class PPTRunspace
    {
        public PPTRunspace(PSObject runspace)
        {
            Id = (int)runspace.Properties["Id"].Value;
            Name = (string)runspace.Properties["Name"].Value;
            //ComputerName = runspace.ConnectionInfo?.ComputerName;
            //Availability = runspace.RunspaceAvailability.ToString();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ComputerName { get; set; }
        public string Availability { get; set; }
    }
}
