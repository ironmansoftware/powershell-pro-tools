using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PowerShellTools
{
    /// <summary>
    /// Interaction logic for UpdateMarginControl.xaml
    /// </summary>
    public partial class UpdateMarginControl : UserControl
    {
        public UpdateMarginControl()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;

            this.Visibility = Visibility.Collapsed;
        }
    }
}
