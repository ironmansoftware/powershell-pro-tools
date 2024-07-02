using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IronmanPowerShellHost
{
    public class NativeResourceManager
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, string lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, uint lpName, uint lpType);

        public const uint RT_RCDATA = 0x00000010;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool EnumResourceNames(IntPtr hModule, uint lpType, EnumResNameDelegate lpEnumFunc, IntPtr lParam);

        public static byte[] GetResourceFromExecutable(uint lpName, uint lpType)
        {
#pragma warning disable IL3000
            return GetResourceFromExecutable(typeof(NativeResourceManager).Assembly.Location, lpName, lpType);
#pragma warning restore  IL3000
        }

        public static byte[] GetResourceFromExecutable(string lpFileName, uint lpName, uint lpType)
        {
            IntPtr hModule = LoadLibrary(lpFileName);
            if (hModule != IntPtr.Zero)
            {
                IntPtr hResource = FindResource(hModule, lpName, lpType);
                if (hResource != IntPtr.Zero)
                {
                    uint resSize = SizeofResource(hModule, hResource);
                    IntPtr resData = LoadResource(hModule, hResource);
                    if (resData != IntPtr.Zero)
                    {
                        byte[] uiBytes = new byte[resSize];
                        IntPtr ipMemorySource = LockResource(resData);
                        Marshal.Copy(ipMemorySource, uiBytes, 0, (int)resSize);
                        return uiBytes;
                    }
                }
            }
            return null;
        }

        public static void EnumResources(string fileName)
        {
            for (uint i = 0; i < 6666; i++)
            {
                IntPtr hModule = LoadLibrary(fileName);
                EnumResourceNames(hModule, i, EnumRes, IntPtr.Zero);
            }

        }

        public delegate bool EnumResNameDelegate(
         IntPtr hModule,
         IntPtr lpszType,
         IntPtr lpszName,
         IntPtr lParam);

        private static bool IS_INTRESOURCE(IntPtr value)
        {
            if (((uint)value) > ushort.MaxValue)
                return false;
            return true;
        }
        private static uint GET_RESOURCE_ID(IntPtr value)
        {
            if (IS_INTRESOURCE(value))
                return (uint)value;
            throw new System.NotSupportedException("value is not an ID!");
        }
        private static string GET_RESOURCE_NAME(IntPtr value)
        {
            if (IS_INTRESOURCE(value))
                return value.ToString();
            return Marshal.PtrToStringUni((IntPtr)value);
        }

        public static bool EnumRes(IntPtr hModule,
      IntPtr lpszType,
      IntPtr lpszName,
      IntPtr lParam)
        {
            var type = GET_RESOURCE_NAME(lpszType);
            var name = GET_RESOURCE_NAME(lpszName);
            return true;
        }
    }
}
