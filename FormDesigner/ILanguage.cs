using PowerShellToolsPro.FormsDesigner;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;

namespace IMS.FormDesigner
{
    public interface ILanguage
    {
        string Id { get; }
        string Name { get; }
        string FileFilter { get; }
        string DefaultExtension { get; }
        string LicenseProduct { get; }
        IEventBindingService GetEventBindingService(IServiceProvider provider, string fileName);
        CodeDomProvider GetCodeDomProvider(string codeFile, string designerFile, EventGenerationType eventGenerationType);
    }
}
