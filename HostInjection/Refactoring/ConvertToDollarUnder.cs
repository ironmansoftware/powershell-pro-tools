using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;


namespace PowerShellProTools.Host.Refactoring
{
    public class ConvertToDollarUnder : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Convert to $_", RefactorType.ConvertToDollarUnder);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var variable = ast.GetAstUnderCursor<VariableExpressionAst>(state);
            return variable != null && variable.VariablePath.UserPath.Equals("PSItem", StringComparison.OrdinalIgnoreCase);
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
                Content = "$_",
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
