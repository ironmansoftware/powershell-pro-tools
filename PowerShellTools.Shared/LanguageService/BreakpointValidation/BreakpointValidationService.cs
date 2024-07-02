using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    internal sealed class BreakpointValidationService
    {
        IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;

        public BreakpointValidationService(IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        internal int ValidateBreakpointLocation(IVsTextBuffer buffer, int lineIndex, ref TextSpan[] codeSpan)
        {
            var script = BreakpointValidationHelper.GetScript(_editorAdaptersFactoryService, buffer);
            var position = BreakpointValidationHelper.GetBreakpointPosition(script, lineIndex);
            var span = BreakpointValidationHelper.MapBreakpointPositionToTextSpan(position);

            codeSpan[0] = span;

            return position.IsValid ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
    }
}
