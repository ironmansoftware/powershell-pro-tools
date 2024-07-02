using Microsoft.VisualStudio.Shell;
using PowerShellProTools.Host;
using PowerShellTools.Options;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PowerShellTools.Commands
{
    public class SwitchPowerShellVersionCommand : ICommand
    {
        public CommandID CommandId => new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidSelectPowerShellVersion);

        public void Execute(object sender, EventArgs args)
        {
            OleMenuCmdEventArgs eventArgs = args as OleMenuCmdEventArgs;
            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero)
                {
                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    Marshal.GetNativeVariantForObject(GeneralOptions.Instance.PowerShellVersion, vOut);
                    return;
                }

                if (inParam == null)
                {
                    return;
                }

                var value = inParam.ToString();
                GeneralOptions.Instance.PowerShellVersion = value;
                GeneralOptions.Instance.Save();
                GeneralOptions.Instance.OnPowerShellVersionChanged();
            }
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = true;
            }
        }
    }

    public class ListPowerShellVersionsCommand : ICommand
    {
        public CommandID CommandId => new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidPowerShellVersionList);

        public void Execute(object sender, EventArgs args)
        {
            OleMenuCmdEventArgs eventArgs = args as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (inParam != null)
                {
                    throw (new ArgumentException("inParam should not be defined")); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    var powershellLocator = new PowerShellLocator();
                    var values = powershellLocator.PowerShellVersions.Keys.ToArray();
                    Marshal.GetNativeVariantForObject(values, vOut);
                }
                else
                {
                    throw (new ArgumentException("outParam is required")); // force an exception to be thrown
                }
            }
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = true;
            }
        }
    }
}
