using Common.Analysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.QuickFix;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace PowerShellTools.Classification
{
    public class ScriptAnalysisTagger : ITagger<ErrorTag>, INotifyTagsChanged
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private ITextBuffer _textBuffer { get; set; }

        internal ScriptAnalysisTagger(ITextBuffer sourceBuffer)
        {
            _textBuffer = sourceBuffer;
            _textBuffer.Properties.AddProperty(typeof(ScriptAnalysisTagger).Name, this);
            _textBuffer.ContentTypeChanged += Buffer_ContentTypeChanged;
            
            if (PowerShellToolsPackage.Instance?.AnalysisService != null)
            {
                PowerShellToolsPackage.Instance.AnalysisService.OnAnalysisResults += AnalysisServiceCallback_OnAnalysisResults;
            }
        }

        internal void AnalysisServiceCallback_OnAnalysisResults(object sender, AnalysisResult e)
        {
            var list = new List<TagInformation<ErrorTag>>();
            if (_textBuffer.Properties.TryGetProperty(BufferProperties.AnalysisErrorTags, out list))
            {
                _textBuffer.Properties.RemoveProperty(BufferProperties.AnalysisErrorTags);
            }

            var snapshot = PowerShellTokenizationService.AnalysisRequests.GetOrAdd(e.RequestId, x => null);

            if (!PowerShellTokenizationService.AnalysisRequests.TryRemove(e.RequestId, out ITextSnapshot currentSnapshot) || currentSnapshot == null)
            {
                return;
            }

            try
            {
                var suggestions = new List<ScriptAnalyzerSuggestion>();
                var tags = new List<TagInformation<ErrorTag>>();

                int issueCounter = 0;

                foreach (var issue in e.Issues)
                {
                    if (currentSnapshot.Length < issue.Line)
                    {
                        break;
                    }

                    var message = issue.Message;

                    var issueStart = currentSnapshot.Lines.ElementAt(issue.Line - 1);
                    var issueEnd = currentSnapshot.Lines.ElementAt(issue.Line - 1);

                    var issueStartPoint = issueStart.Start.Add(issue.Column - 1);
                    var issueEndPoint = issueEnd.End;

                    var tag = new TagInformation<ErrorTag>(issueStartPoint, issueEndPoint - issueStartPoint, new ErrorTag(issue.Severity == "3" ? PredefinedErrorTypeNames.SyntaxError : PredefinedErrorTypeNames.Warning, message));

                    tags.Add(tag);

                    if (issue.Suggestions != null) 
                    {
                        foreach (var suggestion in issue.Suggestions.Where(m => m.Description != null && m.Text != null))
                        {
                            var suggestionStart = currentSnapshot.Lines.ElementAt(suggestion.StartLineNumber - 1);
                            var suggestionEnd = currentSnapshot.Lines.ElementAt(suggestion.EndLineNumber - 1);

                            var startPoint = suggestionStart.Start.Add(suggestion.StartColumnNumber - 1);
                            var endPoint = suggestionEnd.Start.Add(suggestion.EndColumnNumber - 1);

                            var trackingSpan = currentSnapshot.CreateTrackingSpan(startPoint, endPoint - startPoint, SpanTrackingMode.EdgeInclusive);
                            suggestions.Add(new ScriptAnalyzerSuggestion(suggestion.Description, suggestion.Text, trackingSpan));
                        }
                    }

                    issueCounter++;
                }

                if (currentSnapshot.TextBuffer.Properties.TryGetProperty(BufferProperties.ScriptAnalyzerQuickFixer, out ScriptAnalysisSuggestedActionSource scriptAnalysisSuggestedActionSource))
                {
                    scriptAnalysisSuggestedActionSource.OnSuggestedActionsChanged(suggestions);
                }

                if (currentSnapshot.TextBuffer.Properties.ContainsProperty(BufferProperties.AnalysisErrorTags))
                {
                    currentSnapshot.TextBuffer.Properties.RemoveProperty(BufferProperties.AnalysisErrorTags);
                }

                currentSnapshot.TextBuffer.Properties.AddProperty(BufferProperties.AnalysisErrorTags, tags);

                if (currentSnapshot.TextBuffer.Properties.TryGetProperty(BufferProperties.AnalysisTagger, out INotifyTagsChanged classifier))
                {
                    classifier.OnTagsChanged(new SnapshotSpan(currentSnapshot, new Span(0, currentSnapshot.Length)));
                }
            }
            catch
            {

            }
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var currentSnapshot = _textBuffer.CurrentSnapshot;
            if (currentSnapshot.Length == 0) yield break;

            List<TagInformation<ErrorTag>> list;
            _textBuffer.Properties.TryGetProperty(BufferProperties.AnalysisErrorTags, out list);
            if (list == null)
            {
                list = new List<TagInformation<ErrorTag>>();
            }

            foreach (var tagSpan in list.Select(current => current.GetTagSpan(currentSnapshot)).Where(tagSpan => tagSpan != null))
            {
                yield return tagSpan;
            }
        }

        public void OnTagsChanged(SnapshotSpan span)
        {
            var tagsChanged = TagsChanged;
            if (tagsChanged != null)
            {
                tagsChanged(this, new SnapshotSpanEventArgs(span));
            }
        }

        private void Buffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
        {
            _textBuffer.ContentTypeChanged -= Buffer_ContentTypeChanged;
            _textBuffer.Properties.RemoveProperty(typeof(PowerShellErrorTagger).Name);
        }
    }

    [ContentType("TextOutput"), Export(typeof(IViewTaggerProvider)), TagType(typeof(ErrorTag)), ContentType("PowerShell")]
    internal class ScriptAnalysisTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer.Properties.TryGetProperty(typeof(Repl.IPowerShellReplEvaluator), out object obj))
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(typeof(ScriptAnalysisTagger), () => new ScriptAnalysisTagger(buffer) as ITagger<T>);
        }
    }
}
