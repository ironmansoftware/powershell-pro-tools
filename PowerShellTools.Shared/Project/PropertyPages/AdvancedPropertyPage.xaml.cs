using PowerShellProTools.Host;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService;
using PowerShellTools.Shared.Project.PropertyPages;
using PowerShellToolsPro;
using PowerShellToolsPro.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace PowerShellTools.ToolWindows
{
    /// <summary>
    /// Interaction logic for ModulesControl.xaml
    /// </summary>
    public partial class AdvancedPropertyPageControl : UserControl
    {
        public AdvancedPropertyPageControl(IVisualStudio visualStudio)
        {
            InitializeComponent();

            DataContext = new AdvancedPropertyPageViewModel(visualStudio);
        }

        public void BrowseIconClicked(object sender, RoutedEventArgs args)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Icon|*.ico";
            var result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var dataContext = DataContext as AdvancedPropertyPageViewModel;
                dataContext.Icon = openFileDialog.FileName;
            }
        }

        public void SetProperties(AdvancedOptionsPane pane)
        {
            var vm = DataContext as AdvancedPropertyPageViewModel;
            vm.SetProperties(pane);
        }
    }
}
