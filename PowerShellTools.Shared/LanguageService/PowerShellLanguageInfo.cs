using System;
using System.ComponentModel.Design;
using System.Management.Automation.Language;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    /// <summary>
    /// Minimal language service.  Implemented directly rather than using the Managed Package
    /// Framework because we don't want to provide colorization services.  Instead we use the
    /// new Visual Studio 2010 APIs to provide these services.  But we still need this to
    /// provide a code window manager so that we can have a navigation bar (actually we don't, this
    /// should be switched over to using our TextViewCreationListener instead).
    /// </summary>
    [Guid("1C4711F1-3766-4F84-9516-43FA4169CC36")]
    internal sealed class PowerShellLanguageInfo : IVsLanguageInfo, IVsLanguageDebugInfo
    {
        private readonly IServiceContainer _serviceContainer;
        private readonly IComponentModel _componentModel;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private PowerShellLanguagePreferences _langPrefs;

        public PowerShellLanguageInfo(IServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
            _componentModel = serviceContainer.GetService(typeof(SComponentModel)) as IComponentModel;
            _editorAdaptersFactoryService = _componentModel.GetService<IVsEditorAdaptersFactoryService>();

            IVsTextManager textMgr = (IVsTextManager)serviceContainer.GetService(typeof(SVsTextManager));
            if (textMgr != null)
            {
                // Load the initial settings
                var langPrefs = new LANGPREFERENCES[1];
                langPrefs[0].guidLang = typeof(PowerShellLanguageInfo).GUID;
                ErrorHandler.ThrowOnFailure(textMgr.GetUserPreferences(null, null, langPrefs, null));
                _langPrefs = new PowerShellLanguagePreferences(langPrefs[0]);

                // Hook up to the setting change
                Guid guid = typeof(IVsTextManagerEvents2).GUID;
                Microsoft.VisualStudio.OLE.Interop.IConnectionPoint connectionPoint;
                ((Microsoft.VisualStudio.OLE.Interop.IConnectionPointContainer)textMgr).FindConnectionPoint(ref guid, out connectionPoint);
                uint cookie;
                connectionPoint.Advise(_langPrefs, out cookie);
            }
        }

        public PowerShellLanguagePreferences LangPrefs
        {
            get
            {
                return _langPrefs;
            }
        }

        #region IVsLanguageInfo Implementation

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            var model = _serviceContainer.GetService(typeof(SComponentModel)) as IComponentModel;
            var service = model.GetService<IVsEditorAdaptersFactoryService>();

            var statusBar = _serviceContainer.GetService(typeof(SVsStatusbar)) as IVsStatusbar;

            IVsTextView textView;
            if (ErrorHandler.Succeeded(pCodeWin.GetPrimaryView(out textView)))
            {
                ppCodeWinMgr = new CodeWindowManager(pCodeWin, service.GetWpfTextView(textView), statusBar);
                return VSConstants.S_OK;
            }

            ppCodeWinMgr = null;
            return VSConstants.E_FAIL;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            // This is the same extension the language service was
            // registered as supporting.
            pbstrExtensions = PowerShellConstants.PS1File + ";" + PowerShellConstants.PSD1File + ";" + PowerShellConstants.PSM1File;
            return VSConstants.S_OK;
        }


        public int GetLanguageName(out string bstrName)
        {
            // This is the same name the language service was registered with.
            bstrName = PowerShellConstants.LanguageName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// GetColorizer is not implemented because we implement colorization using the new managed APIs.
        /// </summary>
        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        #endregion

        #region IVsLanguageDebugInfo Members

        public int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID)
        {
            pguidLanguageID = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation)
        {
            pbstrMkDoc = null;
            return VSConstants.E_FAIL;
        }

        public int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine, int iCol, out string pbstrName, out int piLineOffset)
        {
            var model = _serviceContainer.GetService(typeof(SComponentModel)) as IComponentModel;
            var service = model.GetService<IVsEditorAdaptersFactoryService>();
            var buffer = service.GetDataBuffer(pBuffer);

            pbstrName = "";
            piLineOffset = iCol;
            return VSConstants.E_FAIL;
        }

        public int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_FAIL;
        }

        public int IsMappedLocation(IVsTextBuffer pBuffer, int iLine, int iCol)
        {
            return VSConstants.E_FAIL;
        }

        public int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames)
        {
            /*if((((RESOLVENAMEFLAGS)dwFlags) & RESOLVENAMEFLAGS.RNF_BREAKPOINT) != 0) {
            // TODO: This should go through the project/analysis and see if we can
            // resolve the names...
            }*/
            ppNames = null;
            return VSConstants.E_FAIL;
        }

        public int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (dte2.ActiveDocument == null || !(LanguageUtilities.IsPowerShellExecutableScriptFile(dte2.ActiveDocument.FullName) || LanguageUtilities.IsPowerShellModuleFile(dte2.ActiveDocument.FullName)))
            {
                return VSConstants.S_FALSE;
            }

            var breakpointValidationService = new BreakpointValidationService(_editorAdaptersFactoryService);

            return breakpointValidationService.ValidateBreakpointLocation(pBuffer, iLine, ref pCodeSpan);
        }

        #endregion
    }
}
