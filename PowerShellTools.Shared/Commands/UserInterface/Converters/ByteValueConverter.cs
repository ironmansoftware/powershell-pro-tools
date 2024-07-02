using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Converts from string to byte and back
    /// </summary>
    public class ByteValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;

            if (String.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if (stringValue != null)
            {
                byte result;
                if (Byte.TryParse(stringValue.Trim(), out result))
                {
                    return result;
                }
            }

            return value;
        }
    }
}
