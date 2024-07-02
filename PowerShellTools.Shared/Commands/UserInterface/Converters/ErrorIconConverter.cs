using System;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PowerShellTools.Commands.UserInterface
{
    internal class ErrorIconConverter : IValueConverter
    {
        private BitmapImage _errorIcon;
        private BitmapImage _warningIcon;
        private const string _assemblyLoaderPrefix = "pack://application:,,,/{0};component/Commands/UserInterface/Resources/";

        public ErrorIconConverter()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var packPrefix = string.Format(_assemblyLoaderPrefix, assemblyName);
            _errorIcon = new BitmapImage(new Uri(packPrefix + "StatusAnnotations_Invalid_Color_16x.png"));
            _warningIcon = new BitmapImage(new Uri(packPrefix + "Warning_yellow_7231_16x16.png"));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ValidationResult validationResult = value as ValidationResult;
            if (validationResult != null)
            {
                if (validationResult.IsWarning)
                {
                    return _warningIcon;
                }
                else
                {
                    return _errorIcon;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
