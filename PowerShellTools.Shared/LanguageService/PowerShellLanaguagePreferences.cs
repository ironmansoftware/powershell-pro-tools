using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    internal sealed class PowerShellLanguagePreferences : IVsTextManagerEvents2
    {
        LANGPREFERENCES _preferences;

        public PowerShellLanguagePreferences(LANGPREFERENCES preferences)
        {
            _preferences = preferences;
        }

        #region IVsTextManagerEvents2 Implementation

        public int OnRegisterMarkerType(int iMarkerType)
        {
            return VSConstants.S_OK;
        }

        public int OnRegisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesBegin()
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesEnd()
        {
            return VSConstants.S_OK;
        }

        public int OnUnregisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnUserPreferencesChanged2(VIEWPREFERENCES2[] pviewPrefs, FRAMEPREFERENCES2[] pframePrefs, LANGPREFERENCES2[] plangPrefs, FONTCOLORPREFERENCES2[] pcolorPrefs)
        {
            if (plangPrefs != null && plangPrefs.Length > 0 && plangPrefs[0].guidLang == _preferences.guidLang)
            {
                _preferences.IndentStyle = plangPrefs[0].IndentStyle;
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region Options Supported in Tools\Options\TextEditor\PowerShell

        public vsIndentStyle IndentMode
        {
            get
            {
                return _preferences.IndentStyle;
            }
        }

        #endregion
    }
}
