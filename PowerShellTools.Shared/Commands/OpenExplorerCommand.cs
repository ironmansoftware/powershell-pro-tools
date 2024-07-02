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
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Provides the command for starting the PowerShell Command Explorer window.
    /// </summary>
    internal class OpenExplorerCommand : ICommand
    {
        public string Description
        {
            get
            {
                return "PowerShell Command Explorer";
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            PowerShellToolsPackage.Instance.ShowExplorerWindow();
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
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidDisplayExplorer);
            }
        }
    }
}
