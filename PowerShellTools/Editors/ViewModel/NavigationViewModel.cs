using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PowerShellTools.Editors.ViewModel
{
    class NavigationViewModel : ViewModelBase
    {
        public NavigationViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }

        public MainWindowViewModel MainWindowViewModel { get; set; }

        public void AddTreeNode(TreeViewItem item)
        {
            
        }
    }
}
