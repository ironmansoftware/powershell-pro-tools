using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Common.Logging;
using PowerShellTools.Intellisense;

namespace PowerShellTools.LanguageService
{
    internal sealed class SmartIndent : ISmartIndent
    {
        private ITextView _textView;
        private PowerShellLanguageInfo _info;
        private IServiceProvider _serviceProvider;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SmartIndent));

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Powershell language service.</param>
        /// <param name="textView">Current active TextView.</param>
        public SmartIndent(IServiceProvider serviceProvider, ITextView textView)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
        }

        private PowerShellLanguageInfo Info
        {
            get
            {
                if (_info == null)
                {
                    _info = (PowerShellLanguageInfo)_serviceProvider.GetService(typeof(PowerShellLanguageInfo));
                }
                return _info;
            }
        }

        /// <summary>
        /// Implementation of the interface.
        /// </summary>
        /// <param name="line">The current line after Enter.</param>
        /// <returns>Desired indentation size.</returns>
        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            // User GetIndentSize() instead of GetTabSize() due to the fact VS always uses Indent Size as a TAB size
            int tabSize = _textView.Options.GetIndentSize();

            if (Info == null) return null;

            switch (Info.LangPrefs.IndentMode)
            {
                case vsIndentStyle.vsIndentStyleNone:
                    return null;

                case vsIndentStyle.vsIndentStyleDefault:
                    return GetDefaultIndentationImp(line, tabSize);

                case vsIndentStyle.vsIndentStyleSmart:
                    return GetSmartIndentationImp(line, tabSize);
            }
            return null;
        }

        public void Dispose() { }

        /// <summary>
        /// Implementation of default indentation.
        /// </summary>
        /// <param name="line">The current line after Enter.</param>
        /// <param name="tabSize">The TAB size.</param>
        /// <returns>Desired indentation size.</returns>
        private int? GetDefaultIndentationImp(ITextSnapshotLine line, int tabSize)
        {
            int lineNumber = line.LineNumber;
            if (lineNumber < 1) return 0;

            string baselineText = null;
            ITextSnapshotLine baseline = null;
            IndentUtilities.SkipPrecedingBlankLines(line, out baselineText, out baseline);
            return IndentUtilities.GetCurrentLineIndentation(baselineText, tabSize);
        }

        /// <summary>
        /// Implementation of smart indentation.
        /// </summary>
        /// <param name="line">The current line after Enter.</param>
        /// <param name="tabSize">The TAB size.</param>
        /// <returns>Desired indentation size.</returns>
        private int? GetSmartIndentationImp(ITextSnapshotLine line, int tabSize)
        {
            int lineNumber = line.LineNumber;
            if (lineNumber < 0) return null;

            string baselineText;
            ITextSnapshotLine baseline;
            char groupStartChar;
            string groupStartLineText;
            IndentUtilities.SkipPrecedingBlankLines(line, out baselineText, out baseline);
            int indentation = IndentUtilities.GetCurrentLineIndentation(baselineText, tabSize);

            // If no group start can be found, follow the default indentation
            if (!FindFirstGroupStart(baseline, out groupStartChar, out groupStartLineText))
            {
                return indentation;
            }

            indentation = IndentUtilities.GetCurrentLineIndentation(groupStartLineText, tabSize);

            // If there is no group end in the current line, or there is one but there are other non-whitespace chars preceding it
            // then add a TAB compared with the indentation of the line of group start.
            SnapshotPoint lastGroupEnd;
            if (!FindFirstGroupEnd(line, groupStartChar, out lastGroupEnd))
            {
                return indentation += tabSize;
            }

            // Approach here as the group end was found and there are only white spaces between line start and the group end.
            // We need to delete all the white spaces and then indent it the size as same as group start line.
            var textBuffer = line.Snapshot.TextBuffer;
            int precedingWhiteSpaces = lastGroupEnd - line.Start;
            if (precedingWhiteSpaces > 0 &&
            !textBuffer.EditInProgress &&
            textBuffer.CurrentSnapshot.Length >= precedingWhiteSpaces)
            {
                textBuffer.Delete(new Span(line.Start, precedingWhiteSpaces));
            }

            return indentation;
        }

        private static bool FindFirstGroupEnd(ITextSnapshotLine line, char groupStartChar, out SnapshotPoint groupEnd)
        {
            string lineText = line.GetText();
            char groupEndChar = Utilities.GetPairedBrace(groupStartChar);
            groupEnd = new SnapshotPoint();

            //walk the entire line
            for (int offset = 0; offset < line.Length; offset++)
            {
                char currentChar = lineText[offset];
                if (currentChar == groupEndChar)
                {
                    groupEnd = new SnapshotPoint(line.Snapshot, line.Start + offset);
                    return true;
                }
                if (!char.IsWhiteSpace(currentChar))
                {
                    return false;
                }
            }
            return false;
        }

        private static bool FindFirstGroupStart(ITextSnapshotLine line, out char groupStartChar, out string groupStartLineText)
        {
            var currentSnapshot = line.Snapshot;
            groupStartLineText = line.GetText();
            int lineNumber = line.LineNumber;
            Stack<char> groupChars = new Stack<char>();
            while (true)
            {
                for (int offset = line.Length - 1; offset >= 0; offset--)
                {
                    char currentChar = groupStartLineText[offset];
                    if (Utilities.IsGroupEnd(currentChar))
                    {
                        groupChars.Push(currentChar);
                    }
                    else if (Utilities.IsGroupStart(currentChar))
                    {
                        if (groupChars.Count == 0)
                        {
                            groupStartChar = currentChar;
                            return true;
                        }

                        if (Utilities.GetPairedBrace(currentChar) == groupChars.Peek())
                        {
                            groupChars.Pop();
                        }
                    }
                }

                if ((--lineNumber) < 0)
                {
                    break;
                }

                line = currentSnapshot.GetLineFromLineNumber(lineNumber);
                groupStartLineText = line.GetText();
            }
            groupStartChar = char.MinValue;
            return false;
        }
    }
}
