using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PowerShellTools.Shared
{
    public static class PSExtensions
    {
        public static T GetValue<T>(this PSObject psobject, string propertyName)
        {
            try
            {
                var property = psobject.Properties[propertyName];
                var value = property.Value;
                if (value is T)
                {
                    return (T)value;
                }

                if (value is PSObject valueObject)
                {
                    return (T)valueObject.BaseObject;
                }
            }
            catch
            {
                return default(T);
            }

            return default(T);
        }
    }
}
