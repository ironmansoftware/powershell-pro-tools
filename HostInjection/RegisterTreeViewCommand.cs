using System;
using System.Management.Automation;
using PowerShellToolsPro.VSCode;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsLifecycle.Register, "VSCodeTreeView")]
    public class RegisterTreeViewCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Label { get; set; }
        [Parameter()]
        public string Description { get; set; }
        [Parameter()]
        public string Tooltip { get; set; }
        [Parameter()]
        public string Icon { get; set; }
        [Parameter()]
        public ScriptBlock LoadChildren { get; set; }
        [Parameter()]
        public ScriptBlock InvokeChild { get; set; }

        protected override void BeginProcessing()
        {
            TreeViewService.Instance.RegisterTreeView(new TreeView {
                Description = Description,
                Icon = Icon,
                InvokeChild = InvokeChild,
                LoadChildren = LoadChildren,
                Label = Label,
                Tooltip = Tooltip
            });
        }

        
    }
}
