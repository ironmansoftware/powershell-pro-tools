using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace PowerShellTools.Classification
{
    internal abstract class Classifier : IClassifier, INotifyTagsChanged
    {
        private readonly ITextBuffer _textBuffer;

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        protected ITextBuffer TextBuffer
        {
            get
            {
                return _textBuffer;
            }
        }

        internal Classifier(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            UpdateClassifierBufferProperty();
            var result = VirtualGetClassificationSpans(span);
            return result;
        }

        public void OnTagsChanged(SnapshotSpan notificationSpan)
        {
            var classificationChanged = ClassificationChanged;
            if (classificationChanged != null)
            {
                classificationChanged(this, new ClassificationChangedEventArgs(notificationSpan));
            }
        }

        protected abstract IList<ClassificationSpan> VirtualGetClassificationSpans(SnapshotSpan span);

        private void UpdateClassifierBufferProperty()
        {
            Classifier classifier;
            if (_textBuffer.Properties.TryGetProperty(typeof(Classifier).Name, out classifier))
            {
                if (classifier == this) return;
                _textBuffer.Properties.RemoveProperty(typeof(Classifier).Name);
                _textBuffer.Properties.AddProperty(typeof(Classifier).Name, this);
            }
            else
            {
                _textBuffer.Properties.AddProperty(typeof(Classifier).Name, this);
            }
        }
    }
}
