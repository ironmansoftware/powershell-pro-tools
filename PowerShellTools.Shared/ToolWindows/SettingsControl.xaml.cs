using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using ModernWpf;
using PowerShellProTools.Host;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PowerShellTools.Shared.ToolWindows
{
    public sealed partial class SettingsControl : System.Windows.Window
    {
        public SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        public SettingsControl(SettingsViewModel viewModel)
        {
            this.InitializeComponent();

            DataContext = viewModel;
            this.Closing += new System.ComponentModel.CancelEventHandler(Window4_Closing);
            btnViewLogs.Click += (e, args) =>
            {
                Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Tools for Visual Studio"));
            };
        }

        void Window4_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SettingsViewModel;
            vm.AnalyzerSettings?.LoadSettings();
        }

        private void SaveAnalysisSettings(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SettingsViewModel;
            vm.AnalyzerSettings.SaveSettings();
        }

        private void About(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ironmansoftware.com/powershell-pro-tools-for-visual-studio");
        }

        private void Docs(object sender, RoutedEventArgs e)
        {
            Process.Start("https://docs.poshtools.com/");
        }

        private void TxtSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.AnalyzerSettings.FilterRules(txtSearch.Text);
        }
    }
}
