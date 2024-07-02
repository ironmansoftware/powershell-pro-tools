//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Intellisense;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace PowerShellTools.LanguageService.BraceCompletion
{
    /// <summary>
    /// Export the implementation for IBraceCompletionContextProvider.
    /// </summary>
    [Export(typeof(IBraceCompletionContextProvider)), ContentType(PowerShellConstants.LanguageName)]
    [BracePair(BraceKind.CurlyBrackets.Open, BraceKind.CurlyBrackets.Close)]
    [BracePair(BraceKind.SquareBrackets.Open, BraceKind.SquareBrackets.Close)]
    [BracePair(BraceKind.Parentheses.Open, BraceKind.Parentheses.Close)]
    [BracePair(BraceKind.SingleQuotes.Open, BraceKind.SingleQuotes.Close)]
    [BracePair(BraceKind.DoubleQuotes.Open, BraceKind.DoubleQuotes.Close)]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider
    {
        [Import]
        private IEditorOperationsFactoryService EditOperationsFactory = null;

        [Import]
        private ITextUndoHistoryRegistry UndoHistoryRegistry = null;

        /// <summary>
        /// Interface method implementation.
        /// </summary>
        /// <param name="textView">Current text view.</param>
        /// <param name="openingPoint">Snapshot point for the opening brace.</param>
        /// <param name="openingBrace">The opening brace char.</param>
        /// <param name="closingBrace">The closing brace char.</param>
        /// <param name="context">The instance of brace completion context.</param>
        /// <returns></returns>
        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            var editorOperations = this.EditOperationsFactory.GetEditorOperations(textView);
            var undoHistory = this.UndoHistoryRegistry.GetHistory(textView.TextBuffer);
            if (IsValidBraceCompletionContext(textView, openingPoint))
            {
                context = new BraceCompletionContext(editorOperations, undoHistory);
                return true;
            }
            else
            {
                context = null;
                return false;
            }
        }

        private bool IsValidBraceCompletionContext(ITextView textView, SnapshotPoint openingPoint)
        {
            Debug.Assert(openingPoint.Position >= 0, "SnapshotPoint.Position should always be zero or positive.");

            // if we are in a comment or string literal we cannot begin a completion session.
            return !Utilities.IsCaretInCommentArea(textView) && !Utilities.IsInStringArea(textView);
        }
    }
}