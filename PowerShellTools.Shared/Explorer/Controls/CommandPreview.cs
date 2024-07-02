using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    public class CommandPreview : TextBox
    {
        static CommandPreview()
        {
            Type ownerType = typeof(CommandPreview);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            TextBlock item = e.OriginalSource as TextBlock;

            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDropHelper.DoDragDrop(item, item.Text, DragDropEffects.Copy);
            }

            base.OnMouseMove(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
