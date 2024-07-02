using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    public class CommandListViewItem : ListViewItem
    {
        static CommandListViewItem()
        {
            //Type ownerType = typeof(CommandListViewItem);
            //DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            CommandListViewItem item = WpfHelper.FindParent<CommandListViewItem>(e.Source as DependencyObject);

            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDropHelper.DoDragDrop(item, item.Content, DragDropEffects.Copy);
            }

            base.OnMouseMove(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
