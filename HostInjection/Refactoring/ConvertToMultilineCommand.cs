using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ConvertToMultiLineCommand : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Convert to multiline command", RefactorType.ConvertToMultiline);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m is CommandAst, true);

            return statement != null;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                 m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                 m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                 m is CommandAst, true);

            if (statement == null)
            {
                yield break;
            }

            var stringBuilder = new StringBuilder();

            var commandAst = statement as CommandAst;
            var commandName = commandAst.GetCommandName();

            var previousIsParameter = false;
            var parameterName = string.Empty;

            var leftOverArguments = new List<string>();

            stringBuilder.Append($"{commandName} ");

            foreach (var commandElement in commandAst.CommandElements.Skip(1))
            {
                if (commandElement is CommandParameterAst parameterAst)
                {
                    if (previousIsParameter)
                    {
                        stringBuilder.AppendFormat("-{0}`{1}", parameterName, Environment.NewLine);
                    }

                    previousIsParameter = true;
                    parameterName = parameterAst.ParameterName;
                }
                else if (previousIsParameter)
                {
                    var commandString = commandElement.ToString();
                    stringBuilder.AppendFormat("-{0} {1}`{2}", parameterName, commandString, Environment.NewLine);
                    previousIsParameter = false;
                }
                else
                {
                    stringBuilder.AppendFormat("{0}`{1}", commandElement, Environment.NewLine);
                }
            }

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = stringBuilder.ToString().Trim().TrimEnd('`'),
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Line = statement.Extent.StartLineNumber - 1,
                    Character = statement.Extent.StartColumnNumber - 1,
                    Index = statement.Extent.StartOffset
                },
                End = new TextPosition
                {
                    Line = statement.Extent.EndLineNumber - 1,
                    Character = statement.Extent.EndColumnNumber - 1,
                    Index = statement.Extent.EndOffset
                },
                FileName = state.FileName
            };
        }
    }
}
