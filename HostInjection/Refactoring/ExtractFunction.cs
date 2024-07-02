using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ExtractFunction : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Extract function", RefactorType.ExtractFunction);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return state.SelectionStart.Line < state.SelectionEnd.Line || state.SelectionEnd.Character > state.SelectionStart.Character;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var startExtent = ast.Find(m =>
               m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
               m.Extent.StartColumnNumber >= state.SelectionStart.Character + 1, true)?.Extent;

            var endExtent = ast.Find(m =>
                m.Extent.EndLineNumber == state.SelectionEnd.Line + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionEnd.Character + 1, true)?.Extent;

            if (startExtent == null || endExtent == null || properties == null)
            {
                yield break;
            }

            var name = properties.FirstOrDefault(m => m.Type == RefactorProperty.Name)?.Value;

            if (name == null)
            {
                name = "MyFunction";
            }

            var assignmentsBeforeSelection = ast.FindAll(m =>
                m.Extent.EndLineNumber < startExtent.StartLineNumber &&
                m is AssignmentStatementAst assignment &&
                assignment.Left is VariableExpressionAst, true)
                .Cast<AssignmentStatementAst>()
                .Select(m => m.Left as VariableExpressionAst);

            var assignmentsAfterSelection = ast.FindAll(m =>
                m.Extent.StartLineNumber > endExtent.EndLineNumber &&
                m is VariableExpressionAst, true)
                .Cast<VariableExpressionAst>();

            var variablesInSelection = ast.FindAll(m =>
                m.Extent.StartLineNumber >= startExtent.StartLineNumber &&
                m.Extent.EndLineNumber <= endExtent.EndLineNumber &&
                m is VariableExpressionAst, true)
                .Cast<VariableExpressionAst>();

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"function {name}");
            stringBuilder.AppendLine("{");

            var parameters = assignmentsBeforeSelection
                .Where(m => variablesInSelection
                    .Any(x => x.VariablePath.UserPath.Equals(m.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase)))
                .Select(m => "$" + m.VariablePath.UserPath);

            if (parameters.Any())
            {
                stringBuilder.AppendLine($"\tparam({parameters.Aggregate((x, y) => x + ", " + y)})");
            }

            stringBuilder.AppendLine(state.Content.Substring(startExtent.StartOffset, endExtent.EndOffset - startExtent.StartOffset));
            stringBuilder.AppendLine("}");

            yield return new TextEdit
            {
                Content = stringBuilder.ToString(),
                Start = new TextPosition
                {
                    Line = startExtent.StartLineNumber - 1,
                    Character = startExtent.StartColumnNumber - 1,
                    Index = startExtent.StartOffset
                },
                End = new TextPosition
                {
                    Line = endExtent.EndLineNumber - 1,
                    Character = endExtent.EndColumnNumber - 1,
                    Index = endExtent.EndOffset
                },
                FileName = state.FileName,
                Type = TextEditType.Replace
            };
        }
    }
}
