using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace PowerShellTools.LanguageService
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(PowerShellConstants.LanguageName)]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        private IServiceProvider _serviceProvider;

        [ImportingConstructor]
        internal SmartIndentProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new SmartIndent(_serviceProvider, textView);
        }
    }
}
