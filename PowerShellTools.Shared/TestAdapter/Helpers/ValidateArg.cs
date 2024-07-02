using System;

namespace PowerShellTools.TestAdapter.Helpers
{
    internal static class ValidateArgs
    {
        public static void NotNull<T>(T obj, string objName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(objName);
            }
        }

        public static void NotNullOrEmpty(string str, string strName)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(strName);
            }
        }
    }
}
