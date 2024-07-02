using ActiproSoftware.Windows.Controls.Grids;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PowerShellTools
{
    public class PSObjectTreeItemAdapter : TreeListBoxItemAdapter
    {
        public override IEnumerable GetChildren(TreeListBox ownerControl, object item)
        {
            var variable = item as Variable;

            if (variable?.HasChildren == true && variable?.Path != "$DTE")
            {
                try
                {
                    return PowerShellToolsPackage.Debugger.GetVariableDetails(variable.Path, -1, -1).ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (item is IEnumerable<Variable> vars)
            {
                return vars;
            }

            return base.GetChildren(ownerControl, item);
        }
    }
}
