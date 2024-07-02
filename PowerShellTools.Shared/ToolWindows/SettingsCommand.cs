using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Shared.ToolWindows;
using System;
using System.ComponentModel.Design;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace PowerShellTools.ToolWindows
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SettingsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = (int)GuidList.SettingsCommandId;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.CmdSetGuid);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModulesCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SettingsCommand(OleMenuCommandService commandService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var oleMenu = sender as OleMenuCommand;

            oleMenu.Enabled = true;
            oleMenu.Visible = true;
            oleMenu.Text = "Settings";
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SettingsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ModulesCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SettingsCommand(commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            PowerShellToolsPackage.Settings.ViewModel.FullSettingsVisibility = Visibility.Visible;
            PowerShellToolsPackage.Settings.ViewModel.AnalyzerSettings.Load();

            PowerShellToolsPackage.Settings.ShowDialog();
        }
    }
}
