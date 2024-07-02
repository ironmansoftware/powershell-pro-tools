/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using PowerShellTools.Repl;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using PowerShellTools.LanguageService;

namespace PowerShellTools.Commands
{
#if POWERSHELL
    using IReplWindow = IPowerShellReplWindow;
    using IReplWindowProvider = IPowerShellReplWindowProvider;
#endif

    /// <summary>
    /// Provides the command for starting the PowerShell Debug REPL window.
    /// </summary>
    class OpenDebugReplCommand : ICommand
    {

        internal static IReplWindow/*!*/ EnsureReplWindow()
        {
            var compModel = CommonPackage.ComponentModel;
            var provider = compModel.GetService<IReplWindowProvider>();

            var window = provider.FindReplWindow("PowerShell");
            if (window == null)
            {
                window = provider.CreateReplWindow(PowerShellToolsPackage.Instance.ContentType, "PowerShell Interactive Window", typeof(PowerShellLanguageInfo).GUID, "PowerShell");
            }
            return window;
        }

        public string Description
        {
            get
            {
                return "PowerShell Interactive Window";
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            var window = (IReplWindow)EnsureReplWindow();
            IVsWindowFrame windowFrame = (IVsWindowFrame)((ToolWindowPane)window).Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            window.Focus();
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var oleMenu = sender as OleMenuCommand;

            oleMenu.Visible = true;
            oleMenu.Enabled = true;
            oleMenu.Supported = true;
        }

        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidDisplayRepl);
            }
        }
    }
}
