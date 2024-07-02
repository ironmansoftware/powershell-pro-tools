using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    [ComVisible(true), CLSCompliant(false)]
    [Guid("F88B684A-5516-489B-BDB4-CDCA6612F45D")]
    public class PowerShellFileNodeProperties : FileNodeProperties
    {
        #region ctors
        internal PowerShellFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }
        #endregion

        #region properties


        [Browsable(false)]
        public string SubType
        {
            get
            {
                return ((PowerShellFileNode)Node).SubType;
            }
            set
            {
                ((PowerShellFileNode)this.Node).SubType = value;
            }
        }

        #endregion
    }
}
