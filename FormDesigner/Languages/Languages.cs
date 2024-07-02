using System;
using System.Linq;

namespace IMS.FormDesigner.Languages
{
    public class Languages
    {
        private static ILanguage[] _languages = new ILanguage[]
        {
            new PowerShellLanguage()
        };

        public static ILanguage GetLanguage(string language)
        {
            return _languages.FirstOrDefault(m => m.Id.Equals(language, StringComparison.OrdinalIgnoreCase));
        }
    }
}
