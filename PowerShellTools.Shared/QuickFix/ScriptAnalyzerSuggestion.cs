using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace PowerShellTools.QuickFix
{
    public class ScriptAnalyzerSuggestion : ISuggestedAction
    {
        private string _text;
        private ITextSnapshot _textSnapshot;

        public ScriptAnalyzerSuggestion(string displayText, string text, ITrackingSpan trackingSpan)
        {
            DisplayText = displayText;
            _text = text;
            TrackingSpan = trackingSpan;
            _textSnapshot = TrackingSpan.TextBuffer.CurrentSnapshot;
        }

        public bool HasActionSets => false;

        public string DisplayText { get; }
        public ITrackingSpan TrackingSpan { get; }

        public ImageMoniker IconMoniker =>  default(ImageMoniker);

        public string IconAutomationText => null;

        public string InputGestureText => null;

        public bool HasPreview => true;

        public void Dispose()
        {
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            var textBlock = new TextBlock();
            textBlock.Padding = new Thickness(5);
            textBlock.Inlines.Add(new Run() { Text = _text });
            return Task.FromResult<object>(textBlock);
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            TrackingSpan.TextBuffer.Replace(TrackingSpan.GetSpan(_textSnapshot), _text);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
