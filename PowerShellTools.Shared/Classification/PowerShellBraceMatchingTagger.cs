using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Classification
{
    internal sealed class PowerShellBraceMatchingTagger : ITagger<HighlightMatchedBracesTag>, INotifyBufferUpdated
    {
        ITextView _textView;
        ITextBuffer _textBuffer;
        SnapshotPoint? _caretPos;
        private IDictionary<int, int> _startBraces;
        private IDictionary<int, int> _endBraces;
        private const string MarkedColor = "blue";
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        internal PowerShellBraceMatchingTagger(ITextView textView, ITextBuffer textBuffer)
        {
            _textView = textView;
            _textBuffer = textBuffer;
            _caretPos = null;
            _textBuffer.Properties.AddProperty(typeof(PowerShellBraceMatchingTagger).Name, this);
            _textBuffer.ContentTypeChanged += TextBuffer_ContentTypeChanged;
            textBuffer.Properties.TryGetProperty<IDictionary<int, int>>(BufferProperties.StartBraces, out _startBraces);
            textBuffer.Properties.TryGetProperty<IDictionary<int, int>>(BufferProperties.EndBraces, out _endBraces);

            _textView.Caret.PositionChanged += CaretPositionChanged;
            _textView.LayoutChanged += ViewLayoutChanged;
        }

        public IEnumerable<ITagSpan<HighlightMatchedBracesTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 ||
            !_caretPos.HasValue ||
            _caretPos.Value.Position > _caretPos.Value.Snapshot.Length ||
            _textView.Caret.InVirtualSpace)
            {
                yield break;
            }

            //hold on to a snapshot of the current character
            SnapshotPoint caretPos = _caretPos.Value;

            //if the requested snapshot isn't the same as the one the brace is on, translate our spans to the expected snapshot
            var caretSnapshot = caretPos.Snapshot;
            if (spans[0].Snapshot != caretSnapshot)
            {
                caretPos = caretPos.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);
            }
            if (caretPos < 0 ||
            caretPos > caretSnapshot.Length ||
            caretPos > spans[0].Snapshot.Length ||
            caretSnapshot.Length == 0 ||
            spans[0].Snapshot.Length == 0)
            {
                yield break;
            }

            char currentChar = '\0';
            if (caretPos < caretSnapshot.Length && caretPos < spans[0].Snapshot.Length)
            {
                currentChar = caretPos.GetChar(); ;
            }

            SnapshotPoint precedingPos = (caretPos == 0 ? caretPos : (caretPos - 1)); //if currentChar is 0 (beginning of buffer), don't move it back 
            char precedingChar = precedingPos.GetChar();
            SnapshotSpan pairSpan = new SnapshotSpan();

            if (caretPos < caretSnapshot.Length && caretPos < spans[0].Snapshot.Length && Utilities.IsLeftBrace(currentChar))
            {
                int closePos;
                char closeChar = Utilities.GetPairedBrace(currentChar);
                if (_startBraces != null && _startBraces.TryGetValue(caretPos, out closePos))
                {
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(caretPos, 1), new HighlightMatchedBracesTag());
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(caretSnapshot, closePos, 1), new HighlightMatchedBracesTag());
                }
                else if (PowerShellBraceMatchingTagger.FindMatchingCloseChar(caretPos, currentChar, closeChar, _textView.TextViewLines.Count, out pairSpan))
                {
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(caretPos, 1), new HighlightMatchedBracesTag());
                    yield return new TagSpan<HighlightMatchedBracesTag>(pairSpan, new HighlightMatchedBracesTag());
                }
            }

            if (precedingPos > 0 && Utilities.IsRightBrace(precedingChar))
            {
                int openPos;
                char openChar = Utilities.GetPairedBrace(precedingChar);
                if (_endBraces != null && _endBraces.TryGetValue(precedingPos, out openPos))
                {
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(precedingPos, 1), new HighlightMatchedBracesTag());
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(caretSnapshot, openPos, 1), new HighlightMatchedBracesTag());
                }
                else if (PowerShellBraceMatchingTagger.FindMatchingOpenChar(precedingPos, openChar, precedingChar, _textView.TextViewLines.Count, out pairSpan))
                {
                    yield return new TagSpan<HighlightMatchedBracesTag>(new SnapshotSpan(precedingPos, 1), new HighlightMatchedBracesTag());
                    yield return new TagSpan<HighlightMatchedBracesTag>(pairSpan, new HighlightMatchedBracesTag());
                }
            }
        }

        public void OnBufferUpdated(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            textBuffer.Properties.TryGetProperty<IDictionary<int, int>>(BufferProperties.StartBraces, out _startBraces);
            textBuffer.Properties.TryGetProperty<IDictionary<int, int>>(BufferProperties.EndBraces, out _endBraces);
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(_textView.Caret.Position);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        void TextBuffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
        {
            _textBuffer.ContentTypeChanged -= TextBuffer_ContentTypeChanged;
            if (_textBuffer.Properties.ContainsProperty(typeof(PowerShellBraceMatchingTagger)))
            {
                _textBuffer.Properties.RemoveProperty(typeof(PowerShellBraceMatchingTagger).Name);
            }

            _textView.Caret.PositionChanged -= CaretPositionChanged;
            _textView.LayoutChanged -= ViewLayoutChanged;
        }

        private void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            _caretPos = caretPosition.Point.GetPoint(_textBuffer, caretPosition.Affinity);

            if (!_caretPos.HasValue)
                return;

            var tagsChanged = TagsChanged;
            if (tagsChanged != null)
            {
                tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0, _textBuffer.CurrentSnapshot.Length)));
            }
        }

        private static bool FindMatchingCloseChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint.Snapshot, 1, 1);
            ITextSnapshotLine line = startPoint.GetContainingLine();
            string lineText = line.GetText();
            int lineNumber = line.LineNumber;
            int offset = startPoint.Position - line.Start.Position + 1;

            int stopLineNumber = startPoint.Snapshot.LineCount - 1;
            if (maxLines > 0)
                stopLineNumber = Math.Min(stopLineNumber, lineNumber + maxLines);

            int openCount = 0;
            while (true)
            {
                //walk the entire line 
                while (offset < line.Length)
                {
                    char currentChar = lineText[offset];
                    if (currentChar == close) //found the close character
                    {
                        if (openCount > 0)
                        {
                            openCount--;
                        }
                        else     //found the matching close
                        {
                            pairSpan = new SnapshotSpan(startPoint.Snapshot, line.Start + offset, 1);
                            return true;
                        }
                    }
                    else if (currentChar == open) // this is another open
                    {
                        openCount++;
                    }
                    offset++;
                }

                //move on to the next line 
                if (++lineNumber > stopLineNumber)
                    break;

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = 0;
            }

            return false;
        }

        private static bool FindMatchingOpenChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint, startPoint);

            ITextSnapshotLine line = startPoint.GetContainingLine();

            int lineNumber = line.LineNumber;
            int offset = startPoint - line.Start - 1; //move the offset to the character before this one 

            //if the offset is negative, move to the previous line 
            if (offset < 0)
            {
                line = line.Snapshot.GetLineFromLineNumber(--lineNumber);
                offset = line.Length - 1;
            }

            string lineText = line.GetText();

            int stopLineNumber = 0;
            if (maxLines > 0)
                stopLineNumber = Math.Max(stopLineNumber, lineNumber - maxLines);

            int closeCount = 0;

            while (true)
            {
                // Walk the entire line 
                while (offset >= 0)
                {
                    char currentChar = lineText[offset];

                    if (currentChar == open)
                    {
                        if (closeCount > 0)
                        {
                            closeCount--;
                        }
                        else // We've found the open character
                        {
                            pairSpan = new SnapshotSpan(line.Start + offset, 1); //we just want the character itself 
                            return true;
                        }
                    }
                    else if (currentChar == close)
                    {
                        closeCount++;
                    }
                    offset--;
                }

                // Move to the previous line 
                if (--lineNumber < stopLineNumber)
                    break;

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = line.Length - 1;
            }
            return false;
        }
    }
}
