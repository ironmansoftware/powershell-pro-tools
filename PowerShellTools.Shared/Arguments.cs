using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    /// <summary>
    /// Utility methods relating to function arguments
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Validates that the argument with the given is non-null, and throws an exception otherwise.
        /// </summary>
        /// <typeparam name="T">The argument type</typeparam>
        /// <param name="value">The argument value to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private IServiceProvider _serviceProvider;
        ///   function MyFunction(IServiceProvider serviceProvider) {
        ///     _serviceProvider = Arguments.ValidateNotNull(serviceProvider, "serviceProvider");
        ///   }
        /// </remarks>
        public static T ValidateNotNull<T>(T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        /// <summary>
        /// Validates that the argument with the given is non-null and contains more than just whitespace, 
        /// and throws an exception otherwise.
        /// </summary>
        /// <param name="value">The argument value to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private string _myString;
        ///   function MyFunction(string myString) {
        ///     _myString = Arguments.ValidateNotNullOrWhitespace(myString, "myString");
        ///   }
        /// </remarks>
        public static string ValidateNotNullOrWhitespace(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        /// <summary>
        /// Validates that of the given arguments, exactly one has been passed in as non-null.
        /// </summary>
        /// <typeparam name="T">The argument type</typeparam>
        /// <param name="value1">The first argument value to validate.</param>
        /// <param name="name1">The name of the first argument, for the exception.</param>
        /// <param name="value2">The second argument value to validate.</param>
        /// <param name="name2">The name of the second argument, for the exception.</param>
        public static void ValidateExactlyOneNotNull<T>(T value1, string name1, T value2, string name2) where T : class
        {
            if ((value1 == null && value2 == null) || (value1 != null && value2 != null))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "{0} {1}", name1, name2));
            }
        }

        /// <summary>
        /// Validates the argument using custom logic and throws if not valid.
        /// </summary>
        /// <param name="isValid">The function to determine whether or not the given value is valid.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private int _myValue;
        ///   function MyFunction(int myValue) {
        ///     _myValue = Arguments.Validate(value => value >= 1, "myValue");
        ///   }
        /// </remarks>
        public static T Validate<T>(T value, Func<T, bool> isValid, string name)
        {
            if (!isValid(value))
            {
                throw new ArgumentException("Invalid argument {0}".FormatCurrentCulture(name));
            }

            return value;
        }

        /// <summary>
        /// Validates an enum argument.
        /// </summary>
        /// <param name="value">The enum to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private MyEnum _myEnum;
        ///   function MyFunction(MyEnum myEnum) {
        ///     _myValue = Arguments.Validate(myEnum, "myValue");
        ///   }
        /// </remarks>
        public static T ValidateEnum<T>(T value, string name) where T : struct
        {
            // Unfortuantely can't constrain to an enum, have to validate that manually
            Arguments.Validate<T>(value, v => v is Enum, name);

            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentException("Invalid arguments {0}".FormatCurrentCulture(name));
            }

            return value;
        }
    }
}
