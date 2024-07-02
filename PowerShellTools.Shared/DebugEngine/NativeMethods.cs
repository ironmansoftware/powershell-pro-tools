using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerShellTools.DebugEngine
{
    /// <summary>
    /// Native methods signatures.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static bool SetForegroundWindow()
        {
            return NativeMethods.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
