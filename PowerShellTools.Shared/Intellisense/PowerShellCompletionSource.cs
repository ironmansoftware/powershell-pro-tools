using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using PowerShellTools.Classification;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Provides the list of possible completion sources for a completion session.
    /// </summary>
    public class PowerShellCompletionSource : ICompletionSource
    {
        private readonly IGlyphService _glyphs;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellCompletionSource));
        private bool _isDisposed;

        public PowerShellCompletionSource(IGlyphService glyphService)
        {
            Log.Debug("Constructor");
            _glyphs = glyphService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var textBuffer = session.TextView.TextBuffer;

            if (!session.Properties.ContainsProperty(BufferProperties.SessionOriginIntellisense)
                || !textBuffer.Properties.ContainsProperty(BufferProperties.LastWordReplacementSpan)
                || !textBuffer.Properties.ContainsProperty(typeof(IList<CompletionResult>))
                || !textBuffer.Properties.ContainsProperty(BufferProperties.LineUpToReplacementSpan))
            {
                return;
            }

            ITrackingSpan trackingSpan;
            IList<CompletionResult> list;
            ITrackingSpan lineStartToApplicableTo;
            textBuffer.Properties.TryGetProperty<ITrackingSpan>(BufferProperties.LastWordReplacementSpan, out trackingSpan);
            textBuffer.Properties.TryGetProperty<IList<CompletionResult>>(typeof(IList<CompletionResult>), out list);
            textBuffer.Properties.TryGetProperty<ITrackingSpan>(BufferProperties.LineUpToReplacementSpan, out lineStartToApplicableTo);

            var currentSnapshot = textBuffer.CurrentSnapshot;
            var trackingSpanEndPoint = trackingSpan.GetEndPoint(currentSnapshot);

            // When IntelliSense for path completion is triggered in double quotes, the quotes are both part of completion, which makes the endpoint of tracking span not boudary of the final completion.
            var endCharPoint = trackingSpanEndPoint - 1;
            char trackingSpanEndPointChar = endCharPoint.GetChar();
            if ( trackingSpanEndPointChar == '\"')
            {
                trackingSpanEndPoint = endCharPoint;
            }
            var filterSpan = currentSnapshot.CreateTrackingSpan(trackingSpanEndPoint, 0, SpanTrackingMode.EdgeInclusive);
            
            var compList = new List<Completion>();
            foreach (var match in list)
            {
                var glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupUnknown, StandardGlyphItem.GlyphItemPublic);
                switch (match.ResultType)
                {
                    case CompletionResultType.ParameterName:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupEnum, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.ParameterValue:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupEnumMember, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Command:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Type:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Property:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Method:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Variable:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.ProviderContainer:
                    case CompletionResultType.ProviderItem:
                        glyph = _glyphs.GetGlyph(match.ResultType == CompletionResultType.ProviderContainer ? StandardGlyphGroup.GlyphOpenFolder : StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);
                        break;
                }

                var completion = new Completion();
                completion.Description = match.ToolTip;
                completion.DisplayText = match.ListItemText;
                completion.InsertionText = match.CompletionText;
                completion.IconSource = glyph;
                completion.IconAutomationText = completion.Description;

                compList.Add(completion);
            }

            completionSets.Add(new PowerShellCompletionSet(string.Empty, string.Empty, trackingSpan, compList, null, filterSpan, lineStartToApplicableTo));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }

    internal class PowerShellCompletionSet : CompletionSet
    {
        private readonly FilteredObservableCollection<Completion> _completions;

        internal PowerShellCompletionSet(string moniker,
                                         string displayName,
                                         ITrackingSpan applicableTo,
                                         IEnumerable<Completion> completions,
                                         IEnumerable<Completion> completionBuilders,
                                         ITrackingSpan filterSpan,
                                         ITrackingSpan lineStartToApplicableTo)
            : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
            if (filterSpan == null)
            {
                throw new ArgumentNullException("filterSpan");
            }
            _completions = new FilteredObservableCollection<Completion>(new ObservableCollection<Completion>(completions));
            FilterSpan = filterSpan;
            LineStartToApplicableTo = lineStartToApplicableTo;
            InitialApplicableTo = applicableTo.GetText(applicableTo.TextBuffer.CurrentSnapshot);
        }

        public override IList<Completion> Completions
        {
            get
            {
                return _completions;
            }
        }

        internal ITrackingSpan FilterSpan { get; private set; }

        internal ITrackingSpan LineStartToApplicableTo { get; private set; }

        internal string InitialApplicableTo { get; private set; }

        public override void Filter()
        {
            var filterText = FilterSpan.GetText(FilterSpan.TextBuffer.CurrentSnapshot);
            Predicate<Completion> predicate = delegate(Completion completion)
            {
                var startIndex = completion.InsertionText.StartsWith(InitialApplicableTo, StringComparison.OrdinalIgnoreCase) ? InitialApplicableTo.Length : 0;
                return completion.InsertionText.IndexOf(filterText, startIndex, StringComparison.OrdinalIgnoreCase) != -1;
            };

            if (Completions.Any(current => predicate(current)))
            {
                _completions.Filter(predicate);
            }
        }

        public override void SelectBestMatch()
        {
            var text = FilterSpan.GetText(FilterSpan.TextBuffer.CurrentSnapshot);

            if (text.Length == 0)
            {
                if (InitialApplicableTo.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                foreach (var current in Completions)
                {
                    if (current.InsertionText.StartsWith(InitialApplicableTo, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectionStatus = new CompletionSelectionStatus(current, true, true);
                        return;
                    }
                }
                return;
            }
            int num = int.MaxValue;
            int matchedCount = 0;
            Completion completion = null;
            foreach (var current in Completions)
            {               
                string currentInsertionText = current.InsertionText;
                if (current.InsertionText.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && current.InsertionText.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    currentInsertionText = currentInsertionText.Substring(1, currentInsertionText.Length - 2);
                }

                var startIndex = current.InsertionText.StartsWith(InitialApplicableTo, StringComparison.OrdinalIgnoreCase) ? this.InitialApplicableTo.Length : 0;
                var num2 = currentInsertionText.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);

                if (num2 != -1 && num2 < num)
                {
                    completion = current;
                    num = num2;
                    ++matchedCount;
                }
            }
            if (completion == null)
            {
                SelectionStatus = new CompletionSelectionStatus(null, false, false);
                return;
            }

            bool isFullyMatched = completion.InsertionText.Equals(InitialApplicableTo + text, StringComparison.OrdinalIgnoreCase);

            var propertiesCollection = FilterSpan.TextBuffer.Properties;
            if (propertiesCollection.ContainsProperty(BufferProperties.SessionCompletionFullyMatchedStatus))
            {
                propertiesCollection.RemoveProperty(BufferProperties.SessionCompletionFullyMatchedStatus);
            }
            propertiesCollection.AddProperty(BufferProperties.SessionCompletionFullyMatchedStatus, isFullyMatched);

            bool isSelected = (matchedCount > 0);
            bool isUnique = (matchedCount == 1);

            SelectionStatus = new CompletionSelectionStatus(completion, isSelected, isUnique);
        }
    }
}
