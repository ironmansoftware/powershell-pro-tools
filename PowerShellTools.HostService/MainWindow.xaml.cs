using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Left = 28000;
            this.Top = 28000;
            this.ShowInTaskbar = false;
            this.Title = string.Format(
                    PowerShellTools.Common.Resources.HostProcessWindowTitleFormat, 
                    App.VsProcessId, 
                    PowerShellTools.Common.Constants.PowerShellHostExeName);

            // We have to visible the window first before hidding it so that we can find the window handle later
            this.Visibility = System.Windows.Visibility.Visible;
            this.Visibility = System.Windows.Visibility.Hidden;

            ConsoleManager.AttachConsole();
        }
    }
}
