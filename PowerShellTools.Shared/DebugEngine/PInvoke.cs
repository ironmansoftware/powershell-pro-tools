using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShellTools.DebugEngine
{
    internal class CredUI
    {
        [DllImport("credui", CharSet = CharSet.Unicode, EntryPoint = "CredUIPromptForCredentialsW")]
        internal static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO pUiInfo, string pszTargetName, IntPtr Reserved, int dwAuthError, StringBuilder pszUserName, int ulUserNameMaxChars, StringBuilder pszPassword, int ulPasswordMaxChars, ref int pfSave, CREDUI_FLAGS dwFlags);

        internal enum CredUIReturnCodes
        {
            NO_ERROR,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004
        }

        internal struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMessageText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [Flags]
        internal enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 1,
            DO_NOT_PERSIST = 2,
            REQUEST_ADMINISTRATOR = 4,
            EXCLUDE_CERTIFICATES = 8,
            REQUIRE_CERTIFICATE = 16,
            SHOW_SAVE_CHECK_BOX = 64,
            ALWAYS_SHOW_UI = 128,
            REQUIRE_SMARTCARD = 256,
            PASSWORD_ONLY_OK = 512,
            VALIDATE_USERNAME = 1024,
            COMPLETE_USERNAME = 2048,
            PERSIST = 4096,
            SERVER_CREDENTIAL = 16384,
            EXPECT_CONFIRMATION = 131072,
            GENERIC_CREDENTIALS = 262144,
            USERNAME_TARGET_CREDENTIALS = 524288,
            KEEP_USERNAME = 1048576
        }
    }
}
