using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellTools.Classification;
using PowerShellTools.Repl;

namespace PowerShellTools.Intellisense
{
    internal static class Utilities
    {
        public static bool IsGroupStart(char ch)
        {
            return ch == '{' || ch == '(';
        }

        public static bool IsGroupEnd(char ch)
        {
            return ch == '}' || ch == ')';
        }

        public static bool IsLeftBrace(char ch)
        {
            return ch == '{' || ch == '[' || ch == '(';
        }

        public static bool IsRightBrace(char ch)
        {
            return ch == '}' || ch == ']' || ch == ')';
        }

        public static bool IsLeftBraceOrQuotes(char ch)
        {
            return IsLeftBrace(ch) || IsQuotes(ch);
        }

        public static bool IsRightBraceOrQuotes(char ch)
        {
            return IsRightBrace(ch) || IsQuotes(ch);
        }

        public static bool IsQuotes(char ch)
        {
            return ch == '\'' || ch == '\"';
        }

        public static bool IsLeftCurlyBrace(char ch)
        {
            return ch == '{';
        }

        public static bool IsRightCurlyBrace(char ch)
        {
            return ch == '}';
        }

        public static char GetCloseBraceOrQuotes(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '(': return ')';
                case '[': return ']';
                case '\'': return '\'';
                case '\"': return '\"';
                default: throw new ArgumentException("ch");
            }
        }

        public static char GetPairedBrace(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '}': return '{';
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
                default: throw new ArgumentException("ch");
            }
        }

        /// <summary>
        /// Determines if caret is in comment area.
        /// </summary>
        /// <param name="textView">Current text view.</param>
        /// <returns>True if caret is in comment area. Otherwise, false.</returns>
        public static bool IsCaretInCommentArea(ITextView textView)
        {
            ITextBuffer currentActiveBuffer;
            int currentPosition = Utilities.GetCurrentBufferPosition(textView, out currentActiveBuffer);
            if (currentPosition < 0 || currentPosition > currentActiveBuffer.CurrentSnapshot.Length)
            {
                return false;
            }
            return Utilities.IsInCommentArea(currentPosition, currentActiveBuffer);
        }

        /// <summary>
        /// Determines if caret is in a string.
        /// </summary>
        /// <returns>True if caret is in a string. Otherwise, false.</returns>
        public static bool IsInStringArea(ITextView textView)
        {
            ITextBuffer currentActiveBuffer;
            int currentPosition = Utilities.GetCurrentBufferPosition(textView, out currentActiveBuffer);
            if (currentPosition < 0 || currentPosition > currentActiveBuffer.CurrentSnapshot.Length)
            {
                return false;
            }
            return Utilities.IsInStringArea(currentPosition, currentActiveBuffer);
        }

        /// <summary>
        /// Get current caret position on a PowerShell textbuffer. If current top text buffer is of type PowerShell, then directly return caret postion.
        /// If current top text buffer is of type REPL, we need to map caret position from REPL text buffer to last PowerShell text buffer and return it. 
        /// If such a mapping doesn't exist, then return -1.
        /// If current top text buffer is neither PowerShell or REPL, we don't deal with it. Just return -1.
        /// </summary>
        /// <param name="textView">The current text view</param>
        /// <param name="currentActiveBuffer">Get the active buffer the caret is on.</param>
        /// <returns>Return the right caret position in a PowerShell text buffer or -1 if none is found.</returns>
        public static int GetCurrentBufferPosition(ITextView textView, out ITextBuffer currentActiveBuffer)
        {
            int currentBufferPosition;
            if (textView.TextBuffer.ContentType.TypeName.Equals(PowerShellConstants.LanguageName, StringComparison.Ordinal))
            {
                currentActiveBuffer = textView.TextBuffer;
                currentBufferPosition = textView.Caret.Position.BufferPosition.Position;
            }
            // If in the REPL window, the current textbuffer won't work, so we have to get the last PowerShell buffer
            else if (textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
            {
                currentActiveBuffer = textView.BufferGraph.GetTextBuffers(p => p.ContentType.TypeName.Equals(PowerShellConstants.LanguageName, StringComparison.Ordinal))
                                                                   .LastOrDefault();

                if (currentActiveBuffer == null)
                {
                    return -1;
                }

                var currentSnapshotPoint = textView.BufferGraph.MapDownToBuffer(textView.Caret.Position.BufferPosition,
                                                                               PointTrackingMode.Positive,
                                                                               currentActiveBuffer,
                                                                               PositionAffinity.Successor);
                if (currentSnapshotPoint != null)
                {
                    currentBufferPosition = currentSnapshotPoint.Value.Position;
                }
                else
                {
                    currentBufferPosition = -1; 
                }
            }
            else
            {
                currentActiveBuffer = null;
                return -1;
            }
            return currentBufferPosition;
        }

        public static bool IsInCommentArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, EdgeTrackingMode.RightEdgeIncluded, PSTokenType.Comment);
        }

        public static bool IsInStringArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, EdgeTrackingMode.NoneEdgeIncluded, PSTokenType.String);
        }

        public static bool IsInParameterArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, EdgeTrackingMode.LeftEdgeIncluded, PSTokenType.CommandParameter);
        }

        public static bool IsInVariableArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, EdgeTrackingMode.LeftEdgeIncluded, PSTokenType.Variable, PSTokenType.Member);
        }

        /// <summary>
        /// Determines if the preceding text in the current line is empty
        /// </summary>
        /// <param name="bufferPosition">The buffer position.</param>
        /// <returns>True if the preceding text is empty.</returns>
        public static bool IsPrecedingTextInLineEmpty(SnapshotPoint bufferPosition)
        {
            var line = bufferPosition.GetContainingLine();
            var caretInLine = ((int)bufferPosition - line.Start);

            return String.IsNullOrWhiteSpace(line.GetText().Substring(0, caretInLine));
        }

        public static bool IsSucceedingTextInLineEmpty(SnapshotPoint bufferPosition)
        {
            var line = bufferPosition.GetContainingLine();
            var caretInLine = ((int)bufferPosition - line.Start);
            string lineText = line.GetText();
            return String.IsNullOrWhiteSpace(lineText.Substring(caretInLine, lineText.Length - caretInLine));
        }

        public static bool IsInNestedExpression(ITextView textView)
        {
            ITextBuffer currentActiveBuffer;
            int currentPosition = Utilities.GetCurrentBufferPosition(textView, out currentActiveBuffer);
            if (currentPosition < 0 || currentPosition > currentActiveBuffer.CurrentSnapshot.Length)
            {
                return false;
            }
            return Utilities.IsInNestedExpression(currentPosition, currentActiveBuffer);
        }


        public static bool IsInNestedExpression(int position, ITextBuffer buffer)
        {
            if (position < 0 || position > buffer.CurrentSnapshot.Length)
            {
                return false;
            }

            Ast ast;
            if (buffer.Properties.TryGetProperty(BufferProperties.Ast, out ast) && ast != null)
            {
                return ast.FindAll(m => position >= m.Extent.StartOffset && position <= m.Extent.EndOffset, true).Any(m => m is SubExpressionAst);
            }

            return false;
        }

        private static bool IsInCertainPSTokenTypesArea(int position, ITextBuffer buffer, EdgeTrackingMode edgeTrackingMode, params PSTokenType[] selectedPSTokenTypes )
        {
            if (position < 0 || position > buffer.CurrentSnapshot.Length)
            {
                return false;
            }

            Token[] tokens;
            if (buffer.Properties.TryGetProperty<Token[]>(BufferProperties.Tokens, out tokens) && tokens != null && tokens.Length != 0)
            {
                var filteredTokens = tokens.Where(t => selectedPSTokenTypes.Any(k => PSToken.GetPSTokenType(t) == k)).ToList();

                switch (edgeTrackingMode)
                {
                    case EdgeTrackingMode.NoneEdgeIncluded:
                    foreach (var token in filteredTokens)
                    {
                            if (token.Extent.StartOffset < position && position < token.Extent.EndOffset)
                        {
                            return true;
                        }
                            if (position <= token.Extent.StartOffset)
                        {
                            return false;
                        }
                    }
                        break;

                    case EdgeTrackingMode.LeftEdgeIncluded:
                        foreach (var token in filteredTokens)
                        {
                            if (token.Extent.StartOffset <= position && position < token.Extent.EndOffset)
                            {
                                return true;
                }
                            if (position < token.Extent.StartOffset)
                {
                                return false;
                            }
                        }
                        break;

                    case EdgeTrackingMode.RightEdgeIncluded:
                    foreach (var token in filteredTokens)
                    {
                            if (token.Extent.StartOffset < position && position <= token.Extent.EndOffset)
                        {
                            return true;
                        }
                            if (position < token.Extent.StartOffset)
                        {
                            return false;
                        }
                    }
                        break;
                
                    case EdgeTrackingMode.BothEdgesIncluded:
                        foreach (var token in filteredTokens)
                        {
                            if (token.Extent.StartOffset <= position && position <= token.Extent.EndOffset)
                            {
                                return true;
            }
                            if (position < token.Extent.StartOffset)
                            {
            return false;
        }
                        }
                        break;

                    default:
                        break;
                }
        }

            return false;
        }
    }
}
