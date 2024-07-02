using System.Collections.Generic;
using System.Text;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class CallTreeNode
    {
        private Dictionary<SequencePoint, CallTreeNode> _children { get; set; }
        internal CallTreeNode Parent { get; }
        public int CallCount { get; set; }
        public long DurationMilliseconds { get; set; }
        public SequencePoint SequencePoint { get; private set; }

        public CallTreeNode() { }

        public CallTreeNode(SequencePoint sequencePoint)
        {
            SequencePoint = sequencePoint;
            _children = new Dictionary<SequencePoint, CallTreeNode>();
            CallCount = 1;
        }

        public CallTreeNode(CallTreeNode parent, SequencePoint sequencePoint)
        {
            SequencePoint = sequencePoint;
            _children = new Dictionary<SequencePoint, CallTreeNode>();
            Parent = parent;
            CallCount = 1;
        }

        public CallTreeNode AddChild(SequencePoint sequencePoint)
        {
            CallTreeNode callTreeNodeChild;
            if (_children.ContainsKey(sequencePoint))
            {
                callTreeNodeChild = _children[sequencePoint];
                callTreeNodeChild.CallCount++;
            }
            else
            {
                callTreeNodeChild = new CallTreeNode(this, sequencePoint);
                _children.Add(sequencePoint, callTreeNodeChild);
            }

            Children = _children.Values;

            return callTreeNodeChild;
        }

        public IEnumerable<CallTreeNode> Children { get; set; }
    }
}
