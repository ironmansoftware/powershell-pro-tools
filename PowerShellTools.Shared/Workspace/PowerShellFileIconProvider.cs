#if !VS17
using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Extensions.VS;

namespace PowerShellTools.Workspace
{
    [Export(typeof(IVsFileIconProvider))]
    class PowerShellFileIconProvider : IVsFileIconProvider
    {
        public AsyncEvent<FileIconsChangedEvent> OnFileIconsChanged { get; set; }

        public bool GetIconForFile(string fullPath, out ImageMoniker imageMoniker, out int priority)
        {
            if (!string.IsNullOrEmpty(fullPath))
            {
                var ext = Path.GetExtension(fullPath);
                if (PowerShellConstants.PS1File.Equals(ext, StringComparison.OrdinalIgnoreCase))
                {
                    imageMoniker = KnownMonikers.PowershellFile;
                    priority = 1000;
                    return true;
                }
            }

            imageMoniker = default(ImageMoniker);
            priority = 0;

            return false;
        }
    }
}
#endif