using System.Management.Automation.Language;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class Timing 
    {
        public Timing(long durationMilliseconds, SequencePoint sequencePoint)
        {
            DurationMilliseconds = durationMilliseconds;
            SequencePoint = sequencePoint;
            CallCount = 1;
        }

        public long DurationMilliseconds { get; set; }
        public SequencePoint SequencePoint { get; set; }
        public long CallCount { get; set; }
    }
}
