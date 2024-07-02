using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Management.Automation;

namespace PowerShellTools.Classification
{
    public class ProfilingTagger : ITagger<ErrorTag>, INotifyTagsChanged
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        private IEnumerable<PSObject> _profilingResults;

        private ITextBuffer _textBuffer { get; set; }

        internal ProfilingTagger(ITextBuffer sourceBuffer)
        {
            _textBuffer = sourceBuffer;
            _textBuffer.Properties.AddProperty(typeof(ProfilingTagger).Name, this);
            _textBuffer.ContentTypeChanged += Buffer_ContentTypeChanged;

            ProfileScriptCommand.ProfilingComplete += ProfileScriptCommand_ProfilingComplete;
        }

        private void ProfileScriptCommand_ProfilingComplete(object sender, DebugEngine.EventArgs<string, IEnumerable<PSObject>> e)
        {
            if (!_textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document)) return;
            
            if (document.FilePath.Equals(e.Value, StringComparison.OrdinalIgnoreCase))
            {
                _profilingResults = e.Value2;
                OnTagsChanged(new SnapshotSpan(_textBuffer.CurrentSnapshot, new Span(0, _textBuffer.CurrentSnapshot.Length)));
            }
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_profilingResults == null) return new ITagSpan<ErrorTag>[0];

            return null;
        }

        public void OnTagsChanged(SnapshotSpan span)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        private void Buffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
        {
            _textBuffer.ContentTypeChanged -= Buffer_ContentTypeChanged;
            _textBuffer.Properties.RemoveProperty(typeof(PowerShellErrorTagger).Name);
        }
    }

    [ContentType("TextOutput"), Export(typeof(IViewTaggerProvider)), TagType(typeof(ErrorTag)), ContentType("PowerShell")]
    internal class ProfilingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer.Properties.TryGetProperty(typeof(Repl.IPowerShellReplEvaluator), out object obj))
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(typeof(ProfilingTagger), () => new ProfilingTagger(buffer) as ITagger<T>);
        }
    }
}
