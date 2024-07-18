using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ConvertToSplat : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Use parameter splatting", RefactorType.ConvertToSplat);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionStart.Character + 1 &&
                m is CommandAst, true) as CommandAst;

            return statement != null && statement.CommandElements.Count > 1;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                 m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                 m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                 m.Extent.EndColumnNumber >= state.SelectionStart.Character + 1 &&
                 m is CommandAst, true);

            if (statement == null)
            {
                yield break;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("$Parameters = @{");

            var commandAst = statement as CommandAst;
            var commandName = commandAst.GetCommandName();

            var previousIsParameter = false;
            var parameterName = string.Empty;

            foreach(var commandElement in commandAst.CommandElements.Skip(1))
            {
                if (commandElement is CommandParameterAst parameterAst)
                {
                    if (previousIsParameter)
                    {
                        stringBuilder.AppendFormat("\t{0} = {1}{2}", parameterName, "$true", Environment.NewLine);
                    }
                    previousIsParameter = true;
                    parameterName = parameterAst.ParameterName;
                    if (parameterAst.Argument != null)
                    {
                        var value = parameterAst.Argument.ToString();
                        if (!value.StartsWith("$"))
                        {
                            value = $"'{value}'";
                        }

                        stringBuilder.AppendFormat("\t{0} = {1}{2}", parameterName, value, Environment.NewLine);
                        previousIsParameter = false;
                    }
                }
                else if (previousIsParameter)
                {
                    var commandString = commandElement.ToString();
                    if (commandElement is StringConstantExpressionAst stringAst &&
                       !stringAst.ToString().StartsWith("\"") &&
                       !stringAst.ToString().StartsWith("\'"))
                    {
                        commandString = $"'{stringAst}'";
                    }

                    stringBuilder.AppendFormat("\t{0} = {1}{2}", parameterName, commandString, Environment.NewLine);
                    previousIsParameter = false;
                }
                else
                {
                    stringBuilder.AppendFormat("\t{0} = {1}{2}", parameterName, "$true", Environment.NewLine);
                }
            }

            if (previousIsParameter)
            {
                stringBuilder.AppendFormat("\t{0} = {1}{2}", parameterName, "$true", Environment.NewLine);
            }

            stringBuilder.AppendLine("}");

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = stringBuilder.ToString(),
                Type = TextEditType.Insert,
                Start = new TextPosition
                {
                    Line = statement.Extent.StartLineNumber - 1,
                    Character = 0,
                    Index = statement.Parent.Extent.StartOffset
                },
                FileName = state.FileName
            };

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = $"{commandName} @Parameters",
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
