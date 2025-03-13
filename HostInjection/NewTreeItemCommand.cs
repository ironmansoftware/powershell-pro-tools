using System;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
  /// <summary>
  /// <para type="synopsis">Creates a new VSCode custom tree item.</para>
  /// <para type="description">This cmdlet creates a new VSCode custom tree item underneath a parent tree view or another tree item.
  /// It's meant to be used in conjunction with the Register-VSCodeTreeView -LoadChildren cmdlet parameter.
  /// </para>
  /// <example>
  ///   <code>New-VSCodeTreeItem -Label "Item1" -Description "This is item 1" -Icon "account" -Tooltip "Hovering over Item1"</code>
  ///   <para>Creates a new treeview item with a label (which doubles as the treeViewId), description, and tooltip.</para>
  /// </example>
  /// <example>
  ///   <code>New-VSCodeTreeItem -Label "Item2" -HasChildren -DisableInvoke</code>
  ///   <para>Creates a new treeview item with the label "Item2" that has children but no invoke button.</para>
  /// </example>
  /// </summary>
  [Cmdlet(VerbsCommon.New, "VSCodeTreeItem")]
    public class NewTreeItemCommand : PSCmdlet
    {
      [Parameter(Mandatory = true, HelpMessage = "The label of the tree item.")]
      public string Label { get; set; }
      [Parameter(HelpMessage = "The description of the tree item. Appears to the right of the label.")]
      public string Description { get; set; }
      [Parameter(HelpMessage = "The tooltip for the tree item.")]
      public string Tooltip { get; set; }
      [Parameter(HelpMessage = "The icon for the tree item, taken from codicons.")]
      public string Icon { get; set; }
      [Parameter(HelpMessage = "Indicates that the tree item can contain children.")]
      public SwitchParameter HasChildren { get; set; }

      [Parameter(HelpMessage = "Indicates that the tree item is not invokable.")]
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
