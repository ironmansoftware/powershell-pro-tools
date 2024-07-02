using IM.CodeDom;
using PowerShellToolsPro.FormsDesigner;
using System;
using System.CodeDom.Compiler;
using System.IO;

namespace IMS.FormDesigner.CodeDom
{
    public class CodeDomProviderFactory
    {
        public static CodeDomProvider GetProvider(string language, string designerFile, string codeFile, EventGenerationType eventGenerationType)
        {
            CodeDomProvider codeDomProvider = null;
            if (language.Equals("powershell", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(designerFile))
            {
                codeDomProvider = new CommandLineCodeDomProvider(codeFile, designerFile, eventGenerationType);
            }

            return codeDomProvider;
        }

        public static CodeDomProvider GetProvider(string designerFile, string codeFile, EventGenerationType eventGenerationType)
        {
            var fileInfo = new FileInfo(designerFile);

            CodeDomProvider codeDomProvider = null;
            if (fileInfo.Extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                codeDomProvider = new CommandLineCodeDomProvider(codeFile, designerFile, eventGenerationType);
            }

            return codeDomProvider;
        }

        public static CodeDomProvider GetDefaultProvider()
        {
            return new CommandLineCodeDomProvider(null, null, EventGenerationType.Variable);
        }
    }
}
