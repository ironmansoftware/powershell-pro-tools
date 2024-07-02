using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    /// <summary>
    /// Enumerates through all programs running in an attachable process. We assume
    /// that there is only one program exists per process though.
    /// </summary>
    internal class RemoteEnumDebugPrograms : IEnumDebugPrograms2
    {
        private ScriptProgramNode _program;

        public RemoteEnumDebugPrograms(ScriptDebugProcess process)
        {
            _program = process.Node;
        }

        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            ppEnum = new RemoteEnumDebugPrograms(_program.Process);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets number of programs retrieved, always either 0 or 1 for our case
        /// </summary>
        /// <param name="pcelt">Out parameter for number of programs</param>
        /// <returns></returns>
        public int GetCount(out uint pcelt)
        {
            pcelt = (_program == null) ? 0u : 1u;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fills the given array with a specified number of programs
        /// </summary>
        /// <param name="celt">How many programs to attempt to retrieve</param>
        /// <param name="rgelt">Array to fill with said programs</param>
        /// <param name="pceltFetched">How many programs were actually put in the array</param>
        /// <returns>If successful, returns S_OK. Returns S_FALSE if fewer than the requested number of elements could be returned</returns>
        public int Next(uint celt, IDebugProgram2[] rgelt, ref uint pceltFetched)
        {
            if (_program == null)
            {
                pceltFetched = 0;
                if (celt > 0)
                {
                    return VSConstants.S_FALSE;
                }
                return VSConstants.S_OK;
            }
            else
            {
                pceltFetched = 1;
                rgelt[0] = _program;
                if (celt > 1)
                {
                    return VSConstants.S_FALSE;
                }
                return VSConstants.S_OK;
            }
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Skips the given number of programs in the enumeration
        /// </summary>
        /// <param name="celt">Number to skip</param>
        /// <returns>If successful, returns S_OK. Returns S_FALSE if celt is greater than the number of remaining elements</returns>
        public int Skip(uint celt)
        {
            if (celt > 1)
            {
                return VSConstants.S_FALSE;
            }
            return VSConstants.S_OK;
        }
    }
}
