using System;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.New, "VSCodeTreeItem")]
    public class NewTreeItemCommand : PSCmdlet
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
        public SwitchParameter HasChildren { get; set; }
        [Parameter()]
        public SwitchParameter DisableInvoke { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new TreeItem {
                Description = Description,
                HasChildren = HasChildren, 
                Icon = Icon, 
                Label = Label,
                Tooltip = Tooltip,
                DisableInvoke = DisableInvoke
            });
        }
    }
}
