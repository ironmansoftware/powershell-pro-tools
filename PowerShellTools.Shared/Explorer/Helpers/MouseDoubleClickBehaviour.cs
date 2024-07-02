using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    public class ClickBehavior
    {
        public static DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached("DoubleClick",
               typeof(ICommand),
               typeof(ClickBehavior),
               new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ClickBehavior.DoubleClickChanged)));

        public static void SetDoubleClick(DependencyObject target, ICommand value)
        {
            target.SetValue(ClickBehavior.DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClick(DependencyObject target)
        {
            return (ICommand)target.GetValue(DoubleClickCommandProperty);
        }

        private static void DoubleClickChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ListViewItem element = target as ListViewItem;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.MouseDoubleClick += OnElementMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.MouseDoubleClick -= OnElementMouseDoubleClick;
                }
            }
        }

        private static void OnElementMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;
            ICommand command = (ICommand)element.GetValue(ClickBehavior.DoubleClickCommandProperty);
            command.Execute(null);
        }
    }
}
