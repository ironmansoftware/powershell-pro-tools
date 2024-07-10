using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using PowerShellToolsPro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PowerShellTools.ToolWindows
{
    /// <summary>
    /// Interaction logic for VariablesControl.
    /// </summary>
    public partial class VariablesControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariablesControl"/> class.
        /// </summary>
        public VariablesControl()
        {
            this.InitializeComponent();

            if (PowerShellToolsPackage.DebuggingService == null)
            {
                PowerShellToolsPackage.DebuggerAvailable += PowerShellToolsPackage_DebuggerAvailable;
            }
            else
            {
                PowerShellToolsPackage.DebuggingService.DebuggerStopped += DebuggingService_ExecuteFinished;
                PowerShellToolsPackage.DebuggingService.ExecuteFinished += DebuggingService_ExecuteFinished;
                RefreshVariables();
            }
        }

        private void PowerShellToolsPackage_DebuggerAvailable(object sender, System.EventArgs e)
        {
            PowerShellToolsPackage.DebuggerAvailable -= PowerShellToolsPackage_DebuggerAvailable;
            PowerShellToolsPackage.DebuggingService.DebuggerStopped += DebuggingService_ExecuteFinished;
            PowerShellToolsPackage.DebuggingService.ExecuteFinished += DebuggingService_ExecuteFinished;
            RefreshVariables();
        }

        private void DebuggingService_ExecuteFinished(object sender, EventArgs e)
        {
            RefreshVariables();
        }

        private void RefreshVariables()
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (ServiceCommon.RunspaceLock)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DataContext = new Variable
                            {
                                StaticChildren = PowerShellToolsPackage.DebuggingService.GetScopedVariable().ToArray()
                            };
                        });
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PowerShellVariables"));
                } catch { }

            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}