using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Converts booleans to visibility values.  Can be inverted and/or told to use Hidden instead of Collapsed.
    /// </summary>
    public class ConfigurableBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true, false maps to visible and true to hidden or collapsed
        /// </summary>
        public bool IsInverted
        {
            get;
            set;
        }

        /// <summary>
        /// If true, hidden is returned instead of collapsed
        /// </summary>
        public bool UseHiddenInsteadOfCollapsed
        {
            get;
            set;
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "interface implementation.")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = System.Convert.ToBoolean(value, culture);
            if (IsInverted)
            {
                boolValue = !boolValue;
            }
            if (boolValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return UseHiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed;
            }
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "interface implementation.")]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
