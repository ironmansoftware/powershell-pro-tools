using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    public class ScrollViewerBehavior
    {
        private static List<MouseWheelEventArgs> _scrollTargets = new List<MouseWheelEventArgs>();

        public static bool GetPassTruScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(PassTruScrollingProperty);
        }

        public static void SetPassTruScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(PassTruScrollingProperty, value);
        }

        public static readonly DependencyProperty PassTruScrollingProperty =
            DependencyProperty.RegisterAttached("PassTruScrolling", 
                typeof(bool), 
                typeof(ScrollViewerBehavior), 
                new FrameworkPropertyMetadata(false, OnPassTruScrollingPropertyChanged));

        public static void OnPassTruScrollingPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewer = sender as ScrollViewer;

            if (viewer == null)
            {
                return;
            }

            if ((bool)e.NewValue == true)
            {
                viewer.PreviewMouseWheel += HandlePreviewMouseWheel;
            }
            else if ((bool)e.NewValue == false)
            {
                viewer.PreviewMouseWheel -= HandlePreviewMouseWheel;
            }
        }

        private static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollControl = sender as ScrollViewer;

            if (scrollControl == null)
            {
                return;
            }

            if (!e.Handled && sender != null && !_scrollTargets.Contains(e))
            {
                var previewEventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent,
                    Source = sender
                };

                var originalSource = e.OriginalSource as UIElement;

                if (originalSource == null)
                {
                    return;
                }

                _scrollTargets.Add(previewEventArg);
                originalSource.RaiseEvent(previewEventArg);
                _scrollTargets.Remove(previewEventArg);

                if (!previewEventArg.Handled && 
                    ((e.Delta > 0 && scrollControl.VerticalOffset == 0) || 
                    (e.Delta <= 0 && scrollControl.VerticalOffset >= scrollControl.ExtentHeight - scrollControl.ViewportHeight)))
                {
                    e.Handled = true;

                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                    eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                    eventArg.Source = sender;

                    var parent = (UIElement)((FrameworkElement)sender).Parent;
                    parent.RaiseEvent(eventArg);
                }
            }
        }
    }
}
