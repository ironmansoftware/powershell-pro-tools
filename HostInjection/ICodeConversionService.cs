using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Common
{
    public interface ICodeConversionService
    {
        string ConvertToPowerShellFromFile(string fileName);

        string ConvertToCSharpFromFile(string fileName);

        string ConvertToPowerShell(string script);
        string ConvertToCSharp(string script);
    }
}
