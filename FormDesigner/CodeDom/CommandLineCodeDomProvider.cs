using PowerShellToolsPro.FormsDesigner;
using System;
using System.IO;

namespace IM.CodeDom
{
    public class CommandLineCodeDomProvider : PowerShellCodeDomProvider
    {
        private readonly string _codeFileName;
        private readonly string _designerFileName;

        public CommandLineCodeDomProvider(string codeFileName, string designerFileName, EventGenerationType eventGenerationType) : base(eventGenerationType)
        {
            _codeFileName = codeFileName;
            _designerFileName = designerFileName;

            var fileInfo = new FileInfo(_designerFileName);

            var starterTemplate = $"Add-Type -AssemblyName System.Windows.Forms" +
                Environment.NewLine +
                $". (Join-Path $PSScriptRoot '{fileInfo.Name}')" +
                Environment.NewLine +
                $"$Form1.ShowDialog()";

            try
            {
                if (File.Exists(_codeFileName))
                {
                    var content = File.ReadAllText(_codeFileName);
                    if (string.IsNullOrEmpty(content))
                    {
                        File.WriteAllText(_codeFileName, starterTemplate);
                    }
                }
                else
                {
                    File.WriteAllText(_codeFileName, starterTemplate);
                }
            }
            catch {  }
        }

        public override string GetCodeFileName(TextReader codeStream)
        {
            return _codeFileName;
        }

        public override string GetCodeFileName(TextWriter codeStream)
        {
            return _codeFileName;
        }

        public override string GetDesignerFileName(TextReader codeStream)
        {
            return _designerFileName;
        }

        public override string GetDesignerFileName(TextWriter codeStream)
        {
            return _designerFileName;
        }

        public override void InsertIntoBeginningOfFile(string fileName, string text)
        {
            
        }

        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
