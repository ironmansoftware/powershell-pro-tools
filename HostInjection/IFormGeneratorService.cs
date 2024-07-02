using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Common
{
    public interface IFormGeneratorService
    {
        void GenerateWinForm(string codeFilePath, string formPath, bool package);
    }
}
