using System;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    public class HostControl : ContentControl
    {
        static HostControl()
        {
           // Type ownerType = typeof(HostControl);
           // DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Gets a value indicating whether the current control,
        /// hosted by this HostControl, is a ISearchTaskTarget.
        /// </summary>
        public bool IsHostingSearchTarget
        {
            get
            {
                return this.Content != null &&
                    this.Content is UserControl &&
                    ((UserControl)this.Content).DataContext is ISearchTaskTarget;
            }
        }
    }
}
