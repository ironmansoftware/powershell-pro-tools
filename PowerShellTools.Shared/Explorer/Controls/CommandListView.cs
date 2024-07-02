using System;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    public class CommandListView : ListView
    {
        static CommandListView()
        {
            //Type ownerType = typeof(CommandListView);
            //DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CommandListViewItem();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
