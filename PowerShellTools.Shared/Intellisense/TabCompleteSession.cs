using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Class that is used when cycling through completions using the tab or backtab command
    /// </summary>
    internal class TabCompleteSession
    {
        private IList<Completion> _completions;
        private int _index;
        private int _startPoint;

        /// <summary>
        /// Creates and initializes the TabCompleteSession
        /// </summary>
        /// <param name="completions">The completions</param>
        /// <param name="selectionStatus">The selection status</param>
        /// <param name="startPoint">The start point of the completions</param>
        public TabCompleteSession(IList<Completion> completions, CompletionSelectionStatus selectionStatus, int startPoint)
        {
            _completions = completions;
            _startPoint = startPoint;

            if (_completions != null && selectionStatus != null && selectionStatus.IsSelected)
            {
                _index = _completions.IndexOf(selectionStatus.Completion);
            }
            else
            {
                _index = -1;
            }
        }

        /// <summary>
        /// Replaces the completion with the next completion
        /// </summary>
        /// <param name="textBuffer">The text buffer</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithNextCompletion(ITextBuffer textBuffer, int caretPosition)
        {
            if (_index < 0)
            {
                // If there was no selected completion, select the first one
                _index = 0;
            }
            else
            {
                _index = ++_index % _completions.Count;
            }

            UpdateCompletion(textBuffer, caretPosition);
        }

        /// <summary>
        /// Replaces the completion with the previous completion
        /// </summary>
        /// <param name="textBuffer">The text buffer</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithPreviousCompletion(ITextBuffer textBuffer, int caretPosition)
        {
            if (_index < 0)
            {
                // If there was no selected completion, select the last one
                _index = _completions.Count - 1;
            }
            else
            {
                _index = (--_index + _completions.Count) % _completions.Count;
            }

            UpdateCompletion(textBuffer, caretPosition);
        }

        private void UpdateCompletion(ITextBuffer textBuffer, int caretPosition)
        {
            var oldCompletionLength = caretPosition - _startPoint;
            var replacementText = _completions[_index].InsertionText;

            textBuffer.Replace(new Span(_startPoint, oldCompletionLength), replacementText);
        }
    }
}
