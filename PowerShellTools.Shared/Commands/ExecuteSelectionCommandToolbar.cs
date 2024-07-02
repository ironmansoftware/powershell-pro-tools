using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Project;

namespace PowerShellTools.Commands
{
    /// <summary>
    ///     This command executes the currently selected code in a PowerShell script.
    /// </summary>
    /// <remarks>
    ///     This command appears in the right-click context menu of a PowerShell script.
    /// </remarks>
    internal class ExecuteSelectionCommandToolbar : ICommand
    {
        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidExecuteSelectionToolbar);
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher();

            Utilities.SaveDirtyFiles();
            TextSelection selection = (TextSelection)dte2.ActiveDocument.Selection;

            // If the selection is completely empty, selected current line and run that.
            if (string.IsNullOrEmpty(selection.Text)) 
            {
                selection.SelectLine();
            }

            launcher.LaunchSelection(selection.Text);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            bool bVisible = ShouldShowCommand(dte2) && 
                dte2.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode;

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }

        private bool ShouldShowCommand(DTE2 dte)
        {
            return dte.ActiveDocument != null &&
                   LanguageUtilities.IsPowerShellExecutableScriptFile(dte.ActiveDocument.FullName);
        }
    }
}
