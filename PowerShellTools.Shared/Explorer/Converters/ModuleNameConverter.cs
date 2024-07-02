using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerShellTools.Explorer
{
    public class ModuleNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;

            if (string.IsNullOrWhiteSpace(s))
            {
                return "<No Module Name>";
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
