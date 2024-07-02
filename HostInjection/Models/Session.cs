using System.Management.Automation.Runspaces;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class Session 
    {
        public Session(PSSession session)
        {
            Id = session.Id;
            ComputerName = session.ComputerName;
            Name = session.Name;
        }

        public Session() {}

        public int Id { get; set; }
        public string ComputerName { get; set; }
        public string Name { get; set; }
    }
}