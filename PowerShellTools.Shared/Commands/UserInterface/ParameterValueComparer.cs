using System;
using System.Security;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    internal static class ParameterValueComparer
    {
        public static bool AreParameterValuesEqual(object value1, object value2)
        {
            // Handle null
            if (value2 == null && value1 == null)
            {
                return true;
            }
            else if (value2 == null || value1 == null)
            {
                return false;
            }
            
            var string1 = value1 as string;
            var string2 = value2 as string;

            // Handle strings
            if (string1 != null && string2 != null)
            {
                return String.Equals(string1, string2, StringComparison.Ordinal);
            }

            // Everything else
            return Object.Equals(value2, value1);
        }
    }
}
