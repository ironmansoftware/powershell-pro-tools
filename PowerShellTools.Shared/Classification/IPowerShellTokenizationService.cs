using System;
using System.Management.Automation.Language;

namespace PowerShellTools.Classification
{
    internal interface IPowerShellTokenizationService
    {
        void StartTokenization();

        event EventHandler<Ast> TokenizationComplete;
    }
}