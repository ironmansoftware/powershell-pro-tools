using Microsoft.VisualStudio.PlatformUI;
using PowerShellTools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace PowerShellTools.Repl.DialogWindows
{
    /// <summary>
    /// Interaction logic for RemoteSessionEnterWindow.xaml
    /// </summary>
    internal partial class RemoteSessionEnterWindow : VsShellDialogWindow
    {
        public RemoteSessionEnterWindow(RemoteSessionWindowViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            InitializeComponent();

            DataContext = viewModel;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
