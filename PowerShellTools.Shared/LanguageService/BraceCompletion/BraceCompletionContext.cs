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

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Operations;
using PowerShellTools.Repl;

namespace PowerShellTools.LanguageService.BraceCompletion
{
    [Export(typeof(IBraceCompletionContext))]
    internal class BraceCompletionContext : IBraceCompletionContext
    {
        private readonly IEditorOperations _editorOperations;
        private readonly ITextUndoHistory _undoHistory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editorOperations">Imported MEF component.</param>
        /// <param name="undoHistory">Imported MEF component.</param>
        public BraceCompletionContext(IEditorOperations editorOperations, ITextUndoHistory undoHistory)
        {
            _editorOperations = editorOperations;
            _undoHistory = undoHistory;
        }

        /// <summary>
        /// Interface method implementation. Called by the editor when the closing brace character has been typed. 
        /// It does not occur if there is any non-whitespace character between the caret and the closing brace.
        /// </summary>
        /// <param name="session">Current brace completion session.</param>
        /// <returns>Always true.</returns>
        public bool AllowOverType(IBraceCompletionSession session)
        {
            return true;
        }

        /// <summary>
        /// Occurs after the session has been removed from the stack.
        /// </summary>
        /// <param name="session"></param>
        public void Finish(IBraceCompletionSession session) { }

        /// <summary>
        /// Called before the session is added to the stack.
        /// </summary>
        /// <param name="session"></param>
        public void Start(IBraceCompletionSession session) { }

        /// <summary>
        /// Called by the editor when return is pressed while both braces are on the same line and no typing has occurred in the session.
        /// </summary>
        /// <param name="session">Current brace completion session.</param>
        public void OnReturn(IBraceCompletionSession session)
        {
            // Return in Repl window would just execute the current command
            if (session.SubjectBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var closingPointPosition = session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot);

            Debug.Assert(
            condition: closingPointPosition > 0,
            message: "The closing point position should always be greater than zero",
            detailMessage: "The closing point position should always be greater than zero, " +
                    "since there is also an opening point for this brace completion session");


            // reshape code from
            // {
            // |}
            // 
            // to
            // {
            //     |
            // }
            // where | indicates caret position.
            using (var undo = _undoHistory.CreateTransaction("Insert new line."))
            {
                _editorOperations.AddBeforeTextBufferChangePrimitive();

                _editorOperations.MoveLineUp(false);
                _editorOperations.MoveToEndOfLine(false);
                _editorOperations.InsertNewLine();

                _editorOperations.AddAfterTextBufferChangePrimitive();
                undo.Complete();
            }
        }
    }
}