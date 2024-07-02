using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using PowerShellToolsPro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

            DataContext = this;

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
            Task.Run(() =>
            {
                try
                {
                    lock (ServiceCommon.RunspaceLock)
                    {
                        PowerShellVariables = PowerShellToolsPackage.DebuggingService.GetScopedVariable().ToArray();
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PowerShellVariables"));
                } catch { }

            });

        }

        public IEnumerable<Variable> PowerShellVariables { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TreeListView_ItemMenuRequested(object sender, ActiproSoftware.Windows.Controls.Grids.TreeListBoxItemMenuEventArgs e)
        {
            try
            {
                var visualStudio = new VisualStudio();
                if (visualStudio.ActiveFile?.IsPowerShellScript == true)
                {
                    e.Menu = new ContextMenu();

                    var menuItem = new MenuItem();
                    menuItem.Click += (s, ev) =>
                    {
                        try
                        {
                            var variable = e.Item as Variable;
                            visualStudio.ActiveFile.InsertAtCaret(variable.Path);
                        }
                        catch (Exception ex)
                        {
                            ServiceCommon.Log(ex.Message);
                        }
                    };

                    menuItem.Header = "Insert Variable";
                    e.Menu.Items.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                ServiceCommon.Log(ex.Message);
            }
        }

    }
}