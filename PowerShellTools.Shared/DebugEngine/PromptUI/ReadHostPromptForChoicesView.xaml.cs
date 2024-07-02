using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PowerShellTools.Common;

namespace PowerShellTools.DebugEngine.PromptUI
{
    /// <summary>
    /// Interaction logic for ReadHostPromptForChoicesView.xaml
    /// </summary>
    internal partial class ReadHostPromptForChoicesView : VsShellDialogWindow
    {
        public ReadHostPromptForChoicesView(ReadHostPromptForChoicesViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            
            this.Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            this.PreviewKeyDown += ReadHostPromptForChoicesView_PreviewKeyDown;
            
            InitializeComponent();
            this.DataContext = viewModel;
        }

        public void ReadHostPromptForChoicesView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape) && (e.KeyboardDevice.Modifiers == ModifierKeys.None))
            {                
                DialogResult = false;
            }
        }

        /// <summary>
        /// Button clicked
        /// </summary>
        /// <param name="sender">The source</param>
        /// <param name="e">Event argument</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
