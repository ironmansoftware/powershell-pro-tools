using System.Collections.Generic;
using System.Diagnostics;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class ProfilingSession
    {
        private Dictionary<SequencePoint, Timing> _timings;

        private CallTreeNode _rootCallTreeNode;
        private CallTreeNode _currentCallTreeNode;

        public Stopwatch Stopwatch { get; }
        private ProfilingSession()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            _timings = new Dictionary<SequencePoint, Timing>();
        }

        public static ProfilingSession Current
        {
            get; private set;
        }

        public static void Start()
        {
            Current = new ProfilingSession();
        }

        public static ProfilingResult Stop()
        {
            var profilingResult = new ProfilingResult
            {
                CallTree = Current._rootCallTreeNode,
                Timings = Current._timings.Values
            };
            
            Current = null;

            return profilingResult;
        }

        public Step Step(string fileName, int startOffset, int endOffset)
        {
            var sequencePoint = new SequencePoint(fileName, startOffset, endOffset);
            return Step(sequencePoint);
        }

        public Step Step(string moduleName, string commandName, string pipelineMethod)
        {
            var sequencePoint = new SequencePoint(moduleName, commandName, pipelineMethod);
            return Step(sequencePoint);
        }

        public Step Step(SequencePoint sequencePoint)
        {
            if (_currentCallTreeNode == null)
            {
                _currentCallTreeNode = _rootCallTreeNode = new CallTreeNode(sequencePoint);
                _currentCallTreeNode.SequencePoint.Root = true;
            }
            else
            {
                _currentCallTreeNode = _currentCallTreeNode.AddChild(sequencePoint);
            }

            return new Step(sequencePoint, this);
        }

        public void AddTiming(Timing timing)
        {
            _currentCallTreeNode.DurationMilliseconds += timing.DurationMilliseconds;
            _currentCallTreeNode = _currentCallTreeNode.Parent;

            if (_timings.ContainsKey(timing.SequencePoint))
            {
                _timings[timing.SequencePoint].CallCount++;
                _timings[timing.SequencePoint].DurationMilliseconds += timing.DurationMilliseconds;
            }
            else
            {
                _timings.Add(timing.SequencePoint, timing);
            }
        }
    }
}
