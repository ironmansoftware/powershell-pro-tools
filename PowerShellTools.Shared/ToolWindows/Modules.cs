using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace PowerShellTools.ToolWindows
{
    [Guid("bd94dc1f-e5ac-47c3-838e-b99d150a4927")]
    public class Modules : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Modules"/> class.
        /// </summary>
        public Modules() : base(null)
        {
            this.Caption = "PowerShell Modules";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ModulesControl();
        }
    }
}
