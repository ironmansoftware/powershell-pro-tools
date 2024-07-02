using ActiproSoftware.Windows.Controls.Grids;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSScriptPad.Common
{
    public class PSObjectTreeItemAdapter : TreeListBoxItemAdapter
    {
        public override IEnumerable GetChildren(TreeListBox ownerControl, object item)
        {
            var variable = item as Variable;

            if (variable?.HasChildren == true)
            {
                using(var ps = PowerShell.Create())
                {
                    ps.Runspace = PowerShellLanguage.
                    ps.AddScript($"Get-PoshToolsVariable -Path '{variable.Path}'");
                    return ps.Invoke<Variable>();
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
