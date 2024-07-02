using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellTools.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.QuickFix
{
    public class ScriptAnalysisSuggestedActionSource : ISuggestedActionsSource
    {
        private readonly ScriptAnalysisSuggestedActionsSourceProvider m_factory;
        private readonly ITextBuffer m_textBuffer;
        private readonly ITextView m_textView;

        private IEnumerable<ScriptAnalyzerSuggestion> _suggestions;

        public ScriptAnalysisSuggestedActionSource(ScriptAnalysisSuggestedActionsSourceProvider testSuggestedActionsSourceProvider, ITextView textView, ITextBuffer textBuffer)
        {
            m_factory = testSuggestedActionsSourceProvider;
            m_textBuffer = textBuffer;
            m_textView = textView;

            if (m_textBuffer.Properties.TryGetProperty(BufferProperties.ScriptAnalyzerQuickFixer, out ISuggestedActionsSource source))
            {
                m_textBuffer.Properties.RemoveProperty(BufferProperties.ScriptAnalyzerQuickFixer);
            }

            m_textBuffer.Properties.AddProperty(BufferProperties.ScriptAnalyzerQuickFixer, this);
        }

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public void OnSuggestedActionsChanged(IEnumerable<ScriptAnalyzerSuggestion> suggestions)
        {
            _suggestions = suggestions;
            SuggestedActionsChanged?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            if (_suggestions != null)
            {
                return new[] { new SuggestedActionSet(_suggestions.Where(m => m.TrackingSpan.GetStartPoint(range.Snapshot) >= range.Start && m.TrackingSpan.GetEndPoint(range.Snapshot) <= range.End)) };
            }
            return Enumerable.Empty<SuggestedActionSet>();
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                if (_suggestions != null)
                {
                    return _suggestions.Any(m => m.TrackingSpan.GetStartPoint(range.Snapshot) >= range.Start && m.TrackingSpan.GetEndPoint(range.Snapshot) <= range.End);
                }
                return false;
            });
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
