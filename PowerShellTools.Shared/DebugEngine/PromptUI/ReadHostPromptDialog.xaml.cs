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

namespace PowerShellTools.DebugEngine.PromptUI
{
    /// <summary>
    /// Interaction logic for ReadHostPromptDialog.xaml
    /// </summary>
    public partial class ReadHostPromptDialog : VsShellDialogWindow
    {
        public ReadHostPromptDialogViewModel _viewModel;

        public ReadHostPromptDialog(ReadHostPromptDialogViewModel viewModel)
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
        /// OK button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
