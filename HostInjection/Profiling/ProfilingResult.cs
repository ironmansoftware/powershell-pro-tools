using System.Collections.Generic;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class ProfilingResult
    {
        public CallTreeNode CallTree { get; set; }
        public IEnumerable<Timing> Timings { get; set; }

        public long TotalDuration => CallTree.DurationMilliseconds;
    }
}
