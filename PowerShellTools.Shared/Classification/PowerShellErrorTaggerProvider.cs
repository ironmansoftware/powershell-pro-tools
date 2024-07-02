using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace PowerShellTools.Classification
{
    [ContentType("TextOutput"), Export(typeof(IViewTaggerProvider)), TagType(typeof(ErrorTag)), ContentType("PowerShell")]
    internal class PowerShellErrorTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(typeof(PowerShellErrorTagger), () => new PowerShellErrorTagger(buffer) as ITagger<T>);
        }
    }
}
