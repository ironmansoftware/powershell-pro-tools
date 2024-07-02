using System;
using System.ComponentModel.Design;
using System.Management.Automation.Language;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Commands.UserInterface;
using PowerShellTools.Project;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script.
    /// </summary>
    /// <remarks>
    /// This command appears in the right-click context menu inside a PowerShell script.
    /// </remarks>
    internal abstract class ExecuteAsScriptCommand : ICommand
    {
        protected abstract int Id { get; }

        protected abstract string GetTargetFile(DTE2 dte);

        protected abstract bool ShouldShowCommand(DTE2 dte);

        protected virtual ScriptParameterResult ScriptArgs { get; set; }

        protected virtual bool ShouldExecuteWithScriptArgs 
        { 
            get
            {
                return false;
            }
        }

        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), Id);
            }
        }

        public virtual void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher();

            var file = GetTargetFile(dte2);

            if (String.IsNullOrEmpty(file))
                return;

            Utilities.SaveDirtyFiles();

            launcher.LaunchFile(file, true, ScriptArgs != null ? ScriptArgs.ScriptArgs : null);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            bool bVisible = ShouldShowCommand(dte2) && dte2.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode;

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }
    }
}
