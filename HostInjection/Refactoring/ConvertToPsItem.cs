using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ConvertToPsItem : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Convert to $PSItem", RefactorType.ConvertToPSItem);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var variable = ast.GetAstUnderCursor<VariableExpressionAst>(state);
            return variable != null && variable.VariablePath.UserPath.Equals("_", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var variable = ast.GetAstUnderCursor<VariableExpressionAst>(state);

            if (variable == null)
            {
                yield break;
            }
            
            yield return new TextEdit
            {
                Content = "$PSItem",
                FileName = state.FileName,
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Character = variable.Extent.StartColumnNumber - 1,
                    Line = variable.Extent.StartLineNumber - 1,
                    Index = variable.Extent.StartOffset
                },
                End = new TextPosition
                {
                    Character = variable.Extent.EndColumnNumber - 1,
                    Line = variable.Extent.EndLineNumber - 1,
                    Index = variable.Extent.EndOffset
                },
            };
        }
    }
}
