using System.Globalization;
using System.Security;

namespace PowerShellTools.Common
{
    /// <summary>
    /// Represents extensions to the String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Formats the string with the current culture.
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        /// <returns>The formatted string</returns>
        public static string FormatCurrentCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Converts a string to a SecureString.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="makeReadOnly">True to make the SecureString read-only. Defaults to true.</param>
        /// <returns>A SecureString containing the specified value. Null if a null string was provided.</returns>
        /// <remarks>
        /// Note that the original string value will remain in memory until it is (eventually) garbage collected.
        /// You should ensure that, once converted, the original value is released as early as possible to ensure it can be garbage collected as soon as possible.
        /// </remarks>
        public static SecureString ToSecureString(this string value, bool makeReadOnly = true)
        {
            if (value == null)
            {
                return null;
            }

            var secureValue = new SecureString();

            foreach (var character in value)
            {
                secureValue.AppendChar(character);
            }

            if (makeReadOnly)
            {
                secureValue.MakeReadOnly();
            }

            return secureValue;
        }
    }
}
