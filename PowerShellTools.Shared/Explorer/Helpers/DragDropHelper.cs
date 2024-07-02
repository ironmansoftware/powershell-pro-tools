using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PowerShellTools.Common;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.Explorer
{
    internal static class DragDropHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DragDropHelper));

        public static void DoDragDrop(DependencyObject element, object obj, DragDropEffects effects)
        {
            var item = obj as IPowerShellCommand;

            if (item != null)
            {
                var content = item.ToString();
                DragDropHelper.DoDragDrop(element, content, DragDropEffects.Copy);
            }
        }

        public static void DoDragDrop(DependencyObject element, string text, DragDropEffects effects)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                DragDrop.DoDragDrop(element, text, DragDropEffects.Copy);
            }
        }
    }
}
