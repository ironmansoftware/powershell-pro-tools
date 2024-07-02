using Microsoft.VisualStudio.Shell.Design.Serialization;
using Microsoft.Win32;
using PowerShellToolsPro.FormsDesigner;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Composition;
using System.IO;

namespace PowerShellToolsPro.FormDesigner
{
    [Export("PowerShellCodeDomProvider", typeof(CodeDomProvider))]
    public class PSCodeDomProvider : PowerShellCodeDomProvider
    {
        private readonly IVisualStudio _visualStudio;

        protected override void Log(string message)
        {
            _visualStudio.Log(LogLevel.Info, message);
        }

        public override string GetDesignerFileName(TextReader codeStream)
        {
            var docDataTextReader = codeStream as DocDataTextReader;

            var fileName = docDataTextReader.GetFileName();
            var project = _visualStudio.Projects.GetProjectContainingFile(fileName);

            _visualStudio.Log(LogLevel.Info, $"PowerShellCodeDomProvider: Parsing {fileName}");

            var referencePath = GetReferencePath();
            project.AddReference(Path.Combine(referencePath, "System.dll"));
            project.AddReference(Path.Combine(referencePath, "System.Windows.Forms.dll"));
            project.AddReference(Path.Combine(referencePath, "System.Drawing.dll"));

            return docDataTextReader.GetDesignerFileName();
        }

        private string GetReferencePath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var v472path = Path.Combine(path, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.7.2");
            var v48path = Path.Combine(path, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.8");

            if (Directory.Exists(v48path))
            {
                return v48path;
            }

            return v472path;
        }

        public override string GetDesignerFileName(TextWriter codeStream)
        {
            var doc = codeStream as DocDataTextWriter;
            return doc.GetDesignerFileName();
        }

        public override string GetCodeFileName(TextReader codeStream)
        {
            var docDataTextReader = codeStream as DocDataTextReader;
            return docDataTextReader.GetFileName();
        }

        public override string GetCodeFileName(TextWriter codeStream)
        {
            var doc = codeStream as DocDataTextWriter;
            return doc.GetFileName();
        }

        public override void InsertIntoBeginningOfFile(string fileName, string text)
        {
            var file = _visualStudio.GetFile(fileName);
            file.InsertAtBeginningOfDocument(text);
        }

        [ImportingConstructor]
        public PSCodeDomProvider(IVisualStudio visualStudio) : base(EventGenerationType.Variable)
        {
            _visualStudio = visualStudio;
        }
    }
}
