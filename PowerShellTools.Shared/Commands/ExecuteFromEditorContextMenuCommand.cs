using System.Diagnostics;
using EnvDTE;
using EnvDTE80;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script from the editor context menu.
    /// </summary>
    internal class ExecuteFromEditorContextMenuCommand : ExecuteAsScriptCommand
    {
        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteAsScript; }
        }

        protected override string GetTargetFile(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);

            if (selectedItem != null && selectedItem.ProjectItem != null)
                return GetPathOfProjectItem(selectedItem.ProjectItem);

            return null;
        }

        protected override bool ShouldShowCommand(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);
            return selectedItem != null &&
                   selectedItem.ProjectItem != null &&
                   LanguageUtilities.IsPowerShellExecutableScriptFile(selectedItem.ProjectItem.Name);
        }

        protected static SelectedItem GetSelectedItem(DTE2 applicationObject)
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
