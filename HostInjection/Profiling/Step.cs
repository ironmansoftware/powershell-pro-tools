using System;
using System.Management.Automation.Language;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class Step : IDisposable
    {
        private readonly long _startMilliseconds;
        private readonly SequencePoint _sequencePoint;
        private readonly ProfilingSession _session;

        public Step(SequencePoint sequencePoint, ProfilingSession profilingSession)
        {
            _startMilliseconds = profilingSession.Stopwatch.ElapsedMilliseconds;
            _sequencePoint = sequencePoint;
            _session = profilingSession;
        }

        public void Dispose()
        {
            _session.AddTiming(new Timing(_session.Stopwatch.ElapsedMilliseconds - _startMilliseconds, _sequencePoint));
        }
    }
}
