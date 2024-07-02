using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.DebugEngine
{
    internal sealed class DebugEngineInternalException: Exception
    {
        public DebugEngineInternalException() { }

        public DebugEngineInternalException(string message) : base(message) { }
    }

}
