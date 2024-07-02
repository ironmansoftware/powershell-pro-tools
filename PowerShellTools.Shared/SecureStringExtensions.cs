using System;
using System.Runtime.InteropServices;
using System.Security;

namespace PowerShellTools.Common
{
    /// <summary>
    /// Extension methods for SecureString type.
    /// </summary>
    internal static class SecureStringExtensions
    {
        /// <summary>
        /// Compares string values of two secure string instances for equality.
        /// </summary>
        /// <param name="secureString1">First secure string.</param>
        /// <param name="secureString2">Second secure string.</param>
        /// <remarks>If both instances are null, they are considered equal.</remarks>
        /// <returns>True if instances are equal; false otherwise.</returns>
        [SecurityCritical]
        public static bool ValueEqualsTo(this SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null && secureString2 == null)
            {
                return true;
            }

            if (secureString1 == null || secureString2 == null)
            {
                return false;
            }

            if ((object)secureString1 == (object)secureString2)
            {
                return true;
            }

            if (secureString1.Length != secureString2.Length)
            {
                return false;
            }

            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;

            // Strings are non-null and has same length. We now compare character by character.
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(secureString1);
                bstr2 = Marshal.SecureStringToBSTR(secureString2);

                for (int i = 0; i < secureString1.Length; i++)
                {
                    // Each BSTR character is 2 bytes
                    int char1 = Marshal.ReadInt16(bstr1 + (i * 2));
                    int char2 = Marshal.ReadInt16(bstr2 + (i * 2));
                    {
                        if (char1 != char2)
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (bstr1 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr1);
                }
                if (bstr2 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr2);
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether the given secure string has a non-empty string value.
        /// </summary>
        /// <param name="secureString">Secure string instance to verify</param>
        /// <returns>True if secure string has non-empty string value, false otherwise.</returns>
        public static bool HasValue(this SecureString secureString)
        {
            return secureString != null && secureString.Length != 0;
        }

        /// <summary>
        /// Checks whether the given secure string has an empty or null value.
        /// </summary>
        /// <param name="secureString">Secure string instance to verify</param>
        /// <returns>True if secure string has non-empty string value, false otherwise.</returns>
        public static bool IsEmptyOrNull(this SecureString secureString)
        {
            return !secureString.HasValue();
        }

        /// <summary>
        /// Converts a SecureString to an unsecure string.  Should be used only as necessary.
        /// </summary>
        /// <param name="secureString">The secure string to convert</param>
        /// <returns>A plaintext, unsecured string</returns>
        [SecurityCritical]
        public static string ConvertToUnsecureString(this SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException("secureString");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
