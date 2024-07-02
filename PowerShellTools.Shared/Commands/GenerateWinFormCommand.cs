using EnvDTE;
using EnvDTE80;
using IMS.FormDesigner;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands
{
    public class GenerateWinFormCommand : ICommand
    {
        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidGenerateWinForm);
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var targetFile = GetTargetFile(dte2);

            var contents = File.ReadAllText(targetFile);

            var formGenerator = new FormGenerator();

            var logic = formGenerator.GenerateLogic(contents, targetFile);
            var form = formGenerator.GenerateForm(ScriptBlock.Create(contents));

            var logicFile = targetFile.Replace(".ps1", "form.ps1");
            var formFile = targetFile.Replace(".ps1", "form.designer.ps1");

            File.WriteAllText(logicFile, logic);
            File.WriteAllText(formFile, form);

            var selectedItem = GetSelectedItem(dte2);
            var projectFile = selectedItem.Project.ProjectItems.AddFromFile(logicFile);
            var formItem = projectFile.ProjectItems.AddFromFile(formFile);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            bool bVisible = ShouldShowCommand(dte2);
            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = false; //bVisible;
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
            Properties projProperties = projItem.Properties;

            try
            {
                string path = projProperties.Item("FullPath").Value as string;    //Item throws if not found.
                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}
