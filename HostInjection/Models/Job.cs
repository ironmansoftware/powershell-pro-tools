using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class PSJob
    {   
        public PSJob() {}
        public PSJob(System.Management.Automation.Job job)
        {
            Id = job.Id;
            Name = job.Name;
            Type = job.PSJobTypeName;
            State = job.JobStateInfo.State.ToString();
            HasMoreData = job.HasMoreData;
            Location = job.Location;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public bool HasMoreData { get; set; }
        public string Location { get; set; }
    }
}