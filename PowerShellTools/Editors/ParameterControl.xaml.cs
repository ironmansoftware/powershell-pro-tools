using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PowerShellTools.Editors;

namespace HelpEditorOS
{
	/// <summary>
	/// Interaction logic for ParameterControl.xaml
	/// </summary>
	public partial class ParametersControl : UserControl
	{
		public ParametersControl()
		{
			this.InitializeComponent();
		}

        /// <summary>
        /// This routine is an event handler. 
        /// I call the save Parameter Description routine everytime a 
        /// LostFocus happened on an editable field in the Cmdlet description page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void saveParameterDescription(object sender, RoutedEventArgs args)
        {
            saveParameterDescription1();    
        }

        /// <summary>
        /// Save parameter data when the Globiing parameter is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobbingParam_Checked(object sender, RoutedEventArgs e)
        {
            saveParameterDescription1();
        }

        /// <summary>
        /// Called when parameters info gets lost focus
        /// </summary>
        public void saveParameterDescription1()
        {
            TreeViewItem Node = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
            if (Node != null)
            {

                parameterDecription param = (parameterDecription)Node.DataContext;
                param.NewDescription = this.ParametersControl1.ParameterDescTextBox.Text;
                param.DefaultValue = this.ParametersControl1.DefaultValueTextBox.Text;
                if ((Boolean)this.ParametersControl1.GlobbingCheckBox.IsChecked)
                {
                    param.Globbing = true;
                }
                else
                {
                    param.Globbing = false;
                }
            }

        }


	}
}