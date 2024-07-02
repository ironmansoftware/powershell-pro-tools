using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;

namespace FormDesigner.Host.Language
{
    public static class ConsoleControl
    {
        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 OPEN_EXISTING = 0x00000003;
        private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        private const UInt32 ERROR_ACCESS_DENIED = 5;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private static void InitializeOutStream()
        {
            var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                var writer = new StreamWriter(fs) { AutoFlush = true };
                System.Console.SetOut(writer);
                System.Console.SetError(writer);
            }
        }

        private static void InitializeInStream()
        {
            var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                System.Console.SetIn(new StreamReader(fs));
            }
        }

        [DllImport("kernel32.dll",
           EntryPoint = "CreateFileW",
           SetLastError = true,
           CharSet = CharSet.Auto,
           CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
             string lpFileName,
             UInt32 dwDesiredAccess,
             UInt32 dwShareMode,
             IntPtr lpSecurityAttributes,
             UInt32 dwCreationDisposition,
             UInt32 dwFlagsAndAttributes,
             IntPtr hTemplateFile
           );

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode, FileAccess dotNetFileAccess)
        {
            var file = new SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                var fs = new FileStream(file, dotNetFileAccess);
                return fs;
            }
            return null;
        }


        public const int SW_HIDE = 0;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();


        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true,
         CharSet = CharSet.Unicode, ExactSpelling = true,
         CallingConvention = CallingConvention.StdCall)]
        public static extern long GetWindowThreadProcessId(long hWnd, long lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        public static extern long GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongA", SetLastError = true)]
        public static extern int SetWindowLongA([System.Runtime.InteropServices.InAttribute()] System.IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOREDRAW = 0x8;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int WS_EX_MDICHILD = 0x40;
        private const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        public const int GWL_STYLE = (-16);
        public const int WS_VISIBLE = 0x10000000;
        private const int WS_CHILD = 0x40000000;

        private static IntPtr hwnd;

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private static CancellationTokenSource exitingTokenSource = new CancellationTokenSource();
        private static CancellationToken exitingToken;
        private static PowerShell readlinePowerShell;

        public static void AllocateConsole()
        {
            if (AllocConsole())
            {
                InitializeOutStream();
                InitializeInStream();
            }

            hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SW_HIDE);
        }

        public static void Reparent(IntPtr newParent)
        {
            ShowWindow(hwnd, SW_SHOW);
            SetParent(hwnd, newParent);
            SetWindowLongA(hwnd, GWL_STYLE, WS_VISIBLE);
        }

        public static void Resize(int width, int height)
        {
            if (hwnd != IntPtr.Zero)
            {
                MoveWindow(hwnd, 0, 0, width, height, true);
            }
        }
    }
}
