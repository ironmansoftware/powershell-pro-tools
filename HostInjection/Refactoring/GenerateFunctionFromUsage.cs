using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class GenerateFunctionFromUsage : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Generate function from usage", RefactorType.GenerateFunctionFromUsage);

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
                 state.SelectionStart.Character + 1 >= m.Extent.StartColumnNumber &&
                 state.SelectionStart.Character + 1 <= m.Extent.EndColumnNumber &&
                 m is CommandAst, true);

            if (statement == null)
            {
                yield break;
            }

            var inPipeline = false;
            if (statement.Parent is PipelineAst pipelineAst)
            {
                int i = 0;
                foreach(var element in pipelineAst.PipelineElements)
                {
                    if (element.Equals(statement) && i > 0)
                    {
                        inPipeline = true;
                        break;
                    }
                    i++;
                }
            }

            var stringBuilder = new StringBuilder();
            var commandAst = statement as CommandAst;
            var commandName = commandAst.GetCommandName();
            stringBuilder.AppendLine("function " + commandName);
            stringBuilder.AppendLine("{");

            if (inPipeline)
            {
                stringBuilder.AppendLine("\t[CmdletBinding()]");
            }

            stringBuilder.AppendLine("\tparam(");

            var previousIsParameter = false;
            var parameterName = string.Empty;

            int elementPosition = 0;

            if (inPipeline)
            {
                stringBuilder.AppendLine("\t\t[Parameter(ValueFromPipeline = $true)]");
                stringBuilder.AppendLine("\t\t$InputObject,");
            }

            foreach (var commandElement in commandAst.CommandElements.Skip(1))
            {
                if (commandElement is CommandParameterAst parameterAst)
                {
                    previousIsParameter = true;
                    parameterName = parameterAst.ParameterName;
                    elementPosition++;
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

                    stringBuilder.AppendLine("\t\t[Parameter()]");
                    stringBuilder.AppendLine($"\t\t${parameterName},");
                    previousIsParameter = false;
                }
                else
                {
                    stringBuilder.AppendLine($"\t\t[Parameter(Position = {elementPosition})]");
                    stringBuilder.AppendLine($"\t\t$Parameter{elementPosition},");
                    elementPosition++;
                }
            }

            // Remove trailing stuff.
            stringBuilder = stringBuilder.Remove(stringBuilder.Length - 3, 3);
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("\t)");

            if (inPipeline)
            {
                stringBuilder.AppendLine("\tProcess");
                stringBuilder.AppendLine("\t{");
                stringBuilder.AppendLine("\t}");
            }

            stringBuilder.AppendLine("}");

            var startLine = statement.Extent.StartLineNumber - 2;

            yield return new TextEdit
            {
                Content = stringBuilder.ToString(),
                Type = TextEditType.Insert,
                Start = new TextPosition
                {
                    Line = startLine < 0 ? 0 : startLine,
                    Character = 0,
                    Index = 0
                },
                FileName = state.FileName
            };
        }
    }
}
