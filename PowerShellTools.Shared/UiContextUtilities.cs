using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    public static class UiContextUtilities
    {
        private static IVsMonitorSelection _monitorSelectionService = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

        /// <summary>
        /// Create VS UI context
        /// </summary>
        /// <param name="guid">UI context guid</param>
        /// <returns>DWORD representation of the GUID identifying the command UI context</returns>
        public static uint CreateUiContext(Guid guid)
        {
            uint uiContextCookie = 0;

            if (_monitorSelectionService != null)
            {
                _monitorSelectionService.GetCmdUIContextCookie(guid, out uiContextCookie);
            }

            return uiContextCookie;
        }

        /// <summary>
        /// Set VS UI context into active
        /// </summary>
        /// <param name="uiContextCookie">DWORD representation of the GUID identifying the command UI context</param>
        public static void ActivateUiContext(uint uiContextCookie)
        {
            if (_monitorSelectionService != null)
            {
                _monitorSelectionService.SetCmdUIContext(uiContextCookie, 1);  // 1 for 'active'
            }
        }

        /// <summary>
        /// Set VS UI context into inactive
        /// </summary>
        /// <param name="uiContextCookie">DWORD representation of the GUID identifying the command UI context</param>
        public static void DeactivateUiContext(uint uiContextCookie)
        {
            if (_monitorSelectionService != null)
            {
                _monitorSelectionService.SetCmdUIContext(uiContextCookie, 0);  // 0 for 'inactive'
            }
        }
    }
}
