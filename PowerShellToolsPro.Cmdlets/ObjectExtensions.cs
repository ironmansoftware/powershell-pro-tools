using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellToolsPro.Cmdlets
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Extension to evaluate an object's ToString() method in an exception safe way. This will
        /// extension method will not throw.
        /// </summary>
        /// <param name="obj">The object on which to call ToString()</param>
        /// <returns>The ToString() return value or a suitable error message is that throws.</returns>
        public static string SafeToString(this object obj)
        {
            string str;

            try
            {
                str = obj.ToString();
            }
            catch (Exception ex)
            {
                str = $"<Error converting property value to string - {ex.Message}>";
            }

            return str;
        }
    }
}
