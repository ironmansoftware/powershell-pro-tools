using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Powershell breakpoint wrapper class with breakpoint Id.
    /// The Id is going to be queried and used for enable/disable/remove breakpoint.
    /// The reason the Id is not directly defined in [DataContract]PowerShellBreakpoint is due to the Id only belongs to powershell runspace,
    /// VS does not have this info and does not care either.
    /// </summary>
    internal sealed class PowerShellBreakpointRecord
    {
        private PowerShellBreakpoint _psBreakpoint;
        private int _id;

        internal PowerShellBreakpointRecord(PowerShellBreakpoint bp, int id)
        {
            _psBreakpoint = bp;
            _id = id;
        }

        /// <summary>
        /// Breakpoint
        /// </summary>
        internal PowerShellBreakpoint PSBreakpoint
        {
            get
            {
                return _psBreakpoint;
            }
        }

        /// <summary>
        /// Breakpoint Id in powershell runspace.
        /// </summary>
        internal int Id
        {
            get
            {
                return _id;
            }
        }

        public bool Equals(PowerShellBreakpointRecord other)
        {
            return this.PSBreakpoint.Equals(other.PSBreakpoint) 
                && this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            PowerShellBreakpointRecord other = obj as PowerShellBreakpointRecord;
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.PSBreakpoint.Equals(other.PSBreakpoint) 
                    && this.Id == other.Id;
            }
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode() 
                ^ PSBreakpoint.ScriptFullPath.GetHashCode() 
                ^ PSBreakpoint.Line.GetHashCode() 
                ^ PSBreakpoint.Column.GetHashCode();
        }
    }
}
