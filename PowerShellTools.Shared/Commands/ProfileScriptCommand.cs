using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.DebugEngine;
using PowerShellTools.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Management.Automation;

namespace PowerShellTools.Commands
{
    internal class ProfileScriptCommand : ICommand
    {
        private string _profiledFile;

        public static event EventHandler<EventArgs<string, IEnumerable<PSObject>>> ProfilingComplete;

        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidProfileScript);
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher();

            var file = GetTargetFile(dte2);

            if (string.IsNullOrEmpty(file))
                return;

            Utilities.SaveDirtyFiles();

            PowerShellToolsPackage.Debugger.DebuggingFinished += Debugger_DebuggingFinished;
            _profiledFile = dte2.ActiveDocument.FullName;

            var commandLine = $"Measure-Script -FilePath '{dte2.ActiveDocument.FullName}'";
            launcher.LaunchSelection(commandLine);
        }

        private void Debugger_DebuggingFinished(object sender, EventArgs e)
        {
            PowerShellToolsPackage.Debugger.DebuggingFinished -= Debugger_DebuggingFinished;
            if (e != null)
            {
                //ProfilingComplete?.Invoke(this, new EventArgs()));
            }
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

        private string GetTargetFile(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);

            if (selectedItem != null && selectedItem.ProjectItem != null)
                return GetPathOfProjectItem(selectedItem.ProjectItem);

            return null;
        }

        private bool ShouldShowCommand(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);
            return selectedItem != null &&
                   selectedItem.ProjectItem != null &&
                   LanguageUtilities.IsPowerShellExecutableScriptFile(selectedItem.ProjectItem.Name);
        }

        private static SelectedItem GetSelectedItem(DTE2 applicationObject)
        {
            if (applicationObject.Solution == null)
                return null;
            if (applicationObject.SelectedItems.Count == 1)
                return applicationObject.SelectedItems.Item(1);
            return null;
        }

        /// <summary>
        /// Returns the path of the project item.  Can return null if path is not found/applicable.
        /// </summary>
        /// <param name="projItem">The project Item</param>
        /// <returns>A string representing the path of the project item.  returns null if path is not found.</returns>
        private static string GetPathOfProjectItem(ProjectItem projItem)
        {
            Debug.Assert(projItem != null, "projItem shouldn't be null");

            Properties projProperties = projItem.Properties;

            try
            {
                string path = projProperties.Item("FullPath").Value as string;    //Item throws if not found.

                Debug.Assert(path != null, "Path isn't a string");

                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}
