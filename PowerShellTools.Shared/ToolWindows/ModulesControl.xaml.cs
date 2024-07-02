using PowerShellProTools.Host;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService;
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
    public partial class ModulesControl : UserControl, INotifyPropertyChanged
    {
        public ModulesControl()
        {
            InitializeComponent();

            DataContext = this;

            if (PowerShellToolsPackage.DebuggingService == null)
            {
                PowerShellToolsPackage.DebuggerAvailable += PowerShellToolsPackage_DebuggerAvailable;
            }
            else
            {
                Refresh();
            }
        }

        private void PowerShellToolsPackage_DebuggerAvailable(object sender, System.EventArgs e)
        {
            PowerShellToolsPackage.DebuggerAvailable -= PowerShellToolsPackage_DebuggerAvailable;
            Refresh();
        }

        private void Refresh()
        {
            RefreshEnabled = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefreshEnabled)));
            Task.Run(() =>
            {
                PowerShellModules = new List<Module> { new Module
                {
                    Name = "Loading..."
                } } ;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerShellModules)));

                lock (ServiceCommon.RunspaceLock)
                {
                    if (_imported)
                    {
                        PowerShellModules = PowerShellToolsPackage.DebuggingService.GetImportedModules().ToArray();
                    }
                    else
                    {
                        PowerShellModules = PowerShellToolsPackage.DebuggingService.GetModules().ToArray();
                    }
                }

                RefreshEnabled = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefreshEnabled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerShellModules)));
            });

        }

        public IEnumerable<Module> PowerShellModules { get; set; }

        public bool RefreshEnabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private bool _imported;

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            _imported = true;
            Refresh();
        }

        private void btnImported_Unchecked(object sender, RoutedEventArgs e)
        {
            _imported = false;
            Refresh();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var module = gridModules.SelectedItem as Module;
            if (module == null) return;

            lock (ServiceCommon.RunspaceLock)
            {
                PowerShellToolsPackage.DebuggingService.ImportModule(module);
            }
        }
    }
}
