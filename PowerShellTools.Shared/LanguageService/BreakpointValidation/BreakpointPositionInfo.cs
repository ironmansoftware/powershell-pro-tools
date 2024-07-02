using System.Management.Automation.Language;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    internal class BreakpointPositionInfo
    {
        public BreakpointPositionInfo(IScriptExtent extent, bool isValid, BreakpointDisplayStyle displayStyle)
        {
            this.Extent = extent;
            this.IsValid = isValid;
            this.DisplayStyle = displayStyle;
        }

        public static BreakpointPositionInfo InvalidBreakpointPosition
        {
            get { return new BreakpointPositionInfo(null, false, BreakpointDisplayStyle.Unset); }
        }

        public IScriptExtent Extent { get; private set; }
        public bool IsValid { get; private set; }
        public BreakpointDisplayStyle DisplayStyle { get; private set; }
    }
}
