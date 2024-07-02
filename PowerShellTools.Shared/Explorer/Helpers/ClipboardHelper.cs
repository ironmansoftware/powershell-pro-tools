using PowerShellTools.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerShellTools.Explorer
{
    internal static class ClipboardHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ClipboardHelper));

        internal static void SetText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (Exception ex)
                {
                    Log.Error("Error setting clipboard text", ex);
                }
            }
            else
            {
                Log.Info("Cannot set empty text to clipboard");
            }
        }
    }
}
