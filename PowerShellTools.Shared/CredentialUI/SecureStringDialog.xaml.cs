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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PowerShellTools.CredentialUI
{
    /// <summary>
    /// Interaction logic for SecureStringDialog.xaml
    /// </summary>
    public partial class SecureStringDialog : VsShellDialogWindow
    {
        SecureStringDialogViewModel _viewModel;

        public SecureStringDialog(SecureStringDialogViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;
        }

        /// <summary>
        /// Password box is friendly with MVVM binding, so we have to separately handle it here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SecString = passwordBox.SecurePassword;
            DialogResult = true;
        }
    }
}
