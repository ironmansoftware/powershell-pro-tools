using IMS.FormDesigner.CodeDom;
using PowerShellToolsPro.FormsDesigner;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;

namespace IMS.FormDesigner.Languages
{
    public class PowerShellLanguage : ILanguage
    {
        public string Name => "PowerShell";

        public string FileFilter => "PowerShell Script|*.ps1";

        public string DefaultExtension => ".cs";

        public string Id => "powershell";

        public string LicenseProduct => "poshprotools";

        public CodeDomProvider GetCodeDomProvider(string codeFile, string designerFile, EventGenerationType eventGenerationType)
        {
            return CodeDomProviderFactory.GetProvider(Name, designerFile, codeFile, eventGenerationType);
        }

        public IEventBindingService GetEventBindingService(IServiceProvider provider, string fileName)
        {
            return new PowerShellEventBindingService(provider, fileName);
        }
    }
}
