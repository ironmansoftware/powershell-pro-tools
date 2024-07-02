using System;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Interaction logic for ParamterEditorView.xaml
    /// </summary>
    internal partial class ParameterEditorView : VsShellDialogWindow
    {
        private ParameterEditorViewModel _viewModel;

        public ParameterEditorView(ParameterEditorViewModel viewModel)
        {
            _viewModel = Arguments.ValidateNotNull(viewModel, "viewModel");

            InitializeComponent();

            this.DataContext = _viewModel;

            _viewModel.ParameterEditingFinished += OnParameterEditingFinished;
        }

        private void OnParameterEditingFinished(object sender, EventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// Raises the closing event.
        /// </summary>
        /// <param name="e">A CancelEventArgs that contains the event data.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.ParameterEditingFinished -= OnParameterEditingFinished;
            _viewModel.Dispose();

            base.OnClosing(e);
        }
    }
}
