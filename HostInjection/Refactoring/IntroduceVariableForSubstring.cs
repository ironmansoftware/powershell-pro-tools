using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class IntroduceVariableForSubstring : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Introduce variable for substring", RefactorType.IntroduceVariableForSubstring);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                state.SelectionStart.Line + 1 >= m.Extent.StartLineNumber &&
                state.SelectionStart.Character + 1 > m.Extent.StartColumnNumber &&
                state.SelectionEnd.Line + 1 <= m.Extent.EndLineNumber &&
                state.SelectionEnd.Character + 1 < m.Extent.EndColumnNumber &&
                m is ExpandableStringExpressionAst, true);

            return statement != null;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                state.SelectionStart.Line + 1 >= m.Extent.StartLineNumber &&
                state.SelectionStart.Character + 1 > m.Extent.StartColumnNumber &&
                state.SelectionEnd.Line + 1 <= m.Extent.EndLineNumber &&
                state.SelectionEnd.Character + 1 < m.Extent.EndColumnNumber &&
                m is ExpandableStringExpressionAst, true) as ExpandableStringExpressionAst;

            if (statement == null)
            {
                yield break;
            }

            yield return new TextEdit
            {
                Content = $"$replacement = '{statement}'",

            };

            yield return new TextEdit
            {
                Content = $"$replacement = '{statement}'",

            };
        }
    }
}
