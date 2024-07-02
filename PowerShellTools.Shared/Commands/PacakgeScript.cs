using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools;
using PowerShellTools.Commands;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Config;
using Task = System.Threading.Tasks.Task;

namespace PowerShellToolsPro.Commands
{
	public class PacakgeScript : ICommand
	{

		public CommandID CommandId
		{
			get
			{
				return new CommandID(new Guid(GuidList.CmdSetGuid), 0x0121);
			}
		}

		public virtual void Execute(object sender, EventArgs args)
		{
			Task.Run(() =>
			{
				ExecuteAsync();
			});
		}

		public void ExecuteAsync()
		{
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

			var file = GetTargetFile(dte2);

			if (string.IsNullOrEmpty(file))
				return;

			var fileInfo = new FileInfo(file);

			if (fileInfo.Extension == ".xaml")
			{
				fileInfo = new FileInfo(file + ".ps1");
			}

			if (!fileInfo.Exists)
				return;

			string outputDirectory = Path.Combine(fileInfo.Directory.FullName, "out");

			if (!Directory.Exists(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}

			//Utilities.SaveDirtyFiles();

			var panes = dte2.ToolWindows.OutputWindow.OutputWindowPanes;
			OutputWindowPane buildPane = null;
			foreach (OutputWindowPane pane in panes)
			{
				if (pane.Name.Contains("Build"))
				{
					buildPane = pane;
					break;
				}
			}

			try
			{
                var config = new PsPackConfig
                {
                    Root = fileInfo.FullName,
                    OutputPath = outputDirectory,
                    Bundle = {
                        Enabled = true,
                        Modules = true,
                        NestedModules = true,
                        RequiredAssemblies = true
                    },
                    Package =
                    {
                        Enabled = true
                    }
                };

                var packageProcess = new PackageProcess();
                packageProcess.OnMessage += (o, s) =>
                {
                    if (buildPane == null) return;
                    buildPane.OutputString(s + Environment.NewLine);
                    buildPane.Activate();
                };
                packageProcess.OnErrorMessage += (o, s) =>
                {
                    if (buildPane == null) return;
                    buildPane.OutputString(s + Environment.NewLine);
                    buildPane.Activate();
                };
                packageProcess.Config = config;
                packageProcess.Execute();
			}
			catch (DotNetNotInstalledException)
			{
				if (buildPane != null)
					buildPane.OutputString(".NET Core SDK is not installed.");

				throw;
			}
			catch (InvalidDotNetVersionException ex)
			{
				if (buildPane != null)
					buildPane.OutputString(ex.Message);

				throw;
			}
		}

		public void QueryStatus(object sender, EventArgs args)
		{
			var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

			bool bVisible = ShouldShowCommand(dte2) && dte2.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode;

			var menuItem = sender as OleMenuCommand;
			if (menuItem != null)
			{
                if (!bVisible)
                {
                    menuItem.Text = menuItem.Text += " (Pro)";
                }
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
			       (selectedItem.ProjectItem.Name.EndsWith(".ps1") ||
			        selectedItem.ProjectItem.Name.EndsWith(".xaml"));
		}

		protected static SelectedItem GetSelectedItem(DTE2 applicationObject)
		{
			// if (applicationObject.Solution == null)
			//    return null;
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
				if (projProperties == null)
				{
					return projItem.FileNames[1];
				}

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
