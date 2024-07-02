using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class Reorder : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Reorder", RefactorType.Reorder);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return false;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var command = ast.GetAstUnderCursor<CommandAst>(state);
            var parameter = ast.GetAstUnderCursor<CommandParameterAst>(state);

            if (command == null || parameter == null)
            {
                yield break;
            }

            var direction = properties.First(mbox => mbox.Type == RefactorProperty.Name).Value;
            var commandSegments = new List<Tuple<Ast, Ast>>();

            var currentParameterIndex = 0;
            var targetParameterIndex = 0;
            Ast previousParameter = null;
            foreach (var element in command.CommandElements.Skip(1))
            {
                if (element is CommandParameterAst parameterAst)
                {
                    if (previousParameter != null)
                    {
                        if (previousParameter == parameter)
                        {
                            targetParameterIndex = currentParameterIndex;
                        }
                        commandSegments.Add(new Tuple<Ast, Ast>(previousParameter, null));
                        currentParameterIndex++;
                    }

                    previousParameter = parameterAst;
                    if (previousParameter == parameter)
                    {
                        targetParameterIndex = currentParameterIndex;
                    }
                }
                else if (previousParameter != null)
                {
                    if (element == parameter)
                    {
                        targetParameterIndex = currentParameterIndex;
                    }
                    commandSegments.Add(new Tuple<Ast, Ast>(previousParameter, element));
                    previousParameter = null;
                    currentParameterIndex++;
                }
                else
                {
                    if (element == parameter)
                    {
                        targetParameterIndex = currentParameterIndex;
                    }
                    commandSegments.Add(new Tuple<Ast, Ast>(element, null));
                    currentParameterIndex++;
                }
            }

            if (previousParameter != null)
            {
                if (previousParameter == parameter)
                {
                    targetParameterIndex = currentParameterIndex;
                }
                commandSegments.Add(new Tuple<Ast, Ast>(previousParameter, null));
            }

            if (direction == "Right")
            {   
                if (targetParameterIndex + 1 == currentParameterIndex)
                {
                    yield break;
                }

                if (commandSegments.Count <= targetParameterIndex +1)
                {
                    yield break;
                }

                var item = commandSegments[targetParameterIndex + 1];
                commandSegments[targetParameterIndex + 1] = commandSegments[targetParameterIndex];
                commandSegments[targetParameterIndex] = item;
            }

            if (direction == "Left")
            {
                if (targetParameterIndex == 0)
                {
                    yield break;
                }

                var item = commandSegments[targetParameterIndex - 1];
                commandSegments[targetParameterIndex - 1] = commandSegments[targetParameterIndex];
                commandSegments[targetParameterIndex] = item;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(command.GetCommandName());
            stringBuilder.Append(" ");

            var cursorPosition = 0;
            var segmentIndex = 0;
            foreach (var segment in commandSegments)
            {
                if (direction == "Left" && segmentIndex == targetParameterIndex - 1)
                {
                    cursorPosition = stringBuilder.Length + 1;
                }

                if (direction == "Right" && segmentIndex == targetParameterIndex + 1)
                {
                    cursorPosition = stringBuilder.Length + 1;
                }

                stringBuilder.Append(segment.Item1.ToString());
                if (segment.Item2 != null)
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(segment.Item2.ToString());
                }
                stringBuilder.Append(" ");
                segmentIndex++;
            }

            stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);

            yield return new TextEdit
            {
                Content = stringBuilder.ToString(),
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Line = command.Extent.StartLineNumber - 1,
                    Character = command.Extent.StartColumnNumber - 1,
                    Index = command.Extent.StartOffset
                },
                End = new TextPosition
                {
                    Line = command.Extent.EndLineNumber - 1,
                    Character = command.Extent.EndColumnNumber - 1,
                    Index = command.Extent.EndOffset
                },
                Cursor = new TextPosition
                {
                    Line = command.Extent.EndLineNumber - 1,
                    Character = cursorPosition
                },
                FileName = state.FileName
            };
        }
    }
}
