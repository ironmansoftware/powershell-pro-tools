using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ExtractVariable : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Extract variable", RefactorType.ExtractVariable);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var result = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m.Extent.EndLineNumber == state.SelectionEnd.Line + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionEnd.Character + 1 &&
                m is StringConstantExpressionAst, true) != null;

            return result;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var startExtent = ast.Find(m =>
               m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
               m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1, true)?.Extent;

            var endExtent = ast.Find(m =>
                m.Extent.EndLineNumber == state.SelectionEnd.Line + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionEnd.Character + 1, true)?.Extent;

            var str = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m.Extent.EndLineNumber == state.SelectionEnd.Line + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionEnd.Character + 1 &&
                m is StringConstantExpressionAst, true) as StringConstantExpressionAst;

            if (startExtent == null || endExtent == null || str == null)
            {
                yield break;
            }

            var name = properties.FirstOrDefault(m => m.Type == RefactorProperty.Name)?.Value;

            if (name == null)
            {
                yield break;
            }

            var startIndex = state.SelectionStart.Character - str.Extent.StartColumnNumber;
            var endIndex = state.SelectionEnd.Character - str.Extent.StartColumnNumber;

            if (startIndex > endIndex) yield break;

            var stringBuilder = new StringBuilder();
            try
            {
                var value = str.Value.Substring(startIndex, endIndex - startIndex);

                stringBuilder.AppendLine($"${name} = '{value}'");
                stringBuilder.Append("\"");
                stringBuilder.Append(str.Value.Substring(0, startIndex));
                stringBuilder.Append($"$(${name})");
                stringBuilder.Append(str.Value.Substring(endIndex));
                stringBuilder.Append("\"");
            }
            catch (Exception)
            {
                yield break;
            }

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
