using System;
using System.ComponentModel;
using System.Globalization;

namespace PowerShellTools.Project.Utils
{
    public class VersionConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                return null;
            }

            return value.ToString();
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var val = value as string;

            if (val == null) return null;

            Version v;

            if (!Version.TryParse(val, out v))
            {
                return null;
            }
            return v;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }


    }
}
