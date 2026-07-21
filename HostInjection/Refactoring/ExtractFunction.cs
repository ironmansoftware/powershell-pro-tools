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

        private static readonly string[] AutomaticVariables = new[]
        {
            "_", "PSItem", "null", "true", "false", "this", "args", "input", "host", "error", "foreach", "switch"
        };

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

            var variablesInSelection = ast.FindAll(m =>
                m.Extent.StartLineNumber >= startExtent.StartLineNumber &&
                m.Extent.EndLineNumber <= endExtent.EndLineNumber &&
                m is VariableExpressionAst, true)
                .Cast<VariableExpressionAst>()
                .Where(m => !AutomaticVariables.Contains(m.VariablePath.UserPath, StringComparer.OrdinalIgnoreCase))
                .OrderBy(m => m.Extent.StartOffset)
                .ToArray();

            var stringBuilder = new StringBuilder();
            var parameters = GetParameters(variablesInSelection);
            var replacementStartOffset = GetLineStartOffset(state.Content, startExtent.StartOffset);
            var replacementStartCharacter = startExtent.StartOffset - replacementStartOffset;
            var baseIndent = state.Content.Substring(replacementStartOffset, replacementStartCharacter);
            if (!baseIndent.All(char.IsWhiteSpace))
            {
                replacementStartOffset = startExtent.StartOffset;
                replacementStartCharacter = startExtent.StartColumnNumber - 1;
                baseIndent = string.Empty;
            }

            const string indent = "    ";

            stringBuilder.AppendLine($"{baseIndent}function {name}");
            stringBuilder.AppendLine($"{baseIndent}{{");
            if (parameters.Any())
            {
                stringBuilder.AppendLine($"{baseIndent}{indent}param(");

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    var suffix = i == parameters.Count - 1 ? string.Empty : ",";
                    stringBuilder.AppendLine($"{baseIndent}{indent}{indent}{parameter.Type}${parameter.Name}{suffix}");
                }

                stringBuilder.AppendLine($"{baseIndent}{indent})");
                stringBuilder.AppendLine();
            }

            var selectedText = state.Content.Substring(replacementStartOffset, endExtent.EndOffset - replacementStartOffset);
            foreach (var line in NormalizeIndent(selectedText, baseIndent))
            {
                stringBuilder.AppendLine($"{baseIndent}{indent}{line}");
            }

            stringBuilder.AppendLine($"{baseIndent}}}");

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = stringBuilder.ToString(),
                Start = new TextPosition
                {
                    Line = startExtent.StartLineNumber - 1,
                    Character = replacementStartCharacter,
                    Index = replacementStartOffset
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

        private static List<ParameterInfo> GetParameters(IEnumerable<VariableExpressionAst> variables)
        {
            var assignedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parameters = new List<ParameterInfo>();

            foreach (var variable in variables)
            {
                var variableName = variable.VariablePath.UserPath;
                var assignment = variable.FindParent<AssignmentStatementAst>();
                var isAssignmentTarget = assignment?.Left.FindAll(m => ReferenceEquals(m, variable), true).Any() == true || ReferenceEquals(assignment?.Left, variable);

                if (isAssignmentTarget)
                {
                    if (assignment.Operator != TokenKind.Equals && !assignedVariables.Contains(variableName))
                    {
                        AddParameter(parameters, variable);
                    }

                    if (assignment.Right.FindAll(m => m is VariableExpressionAst rightVariable &&
                        rightVariable.VariablePath.UserPath.Equals(variableName, StringComparison.OrdinalIgnoreCase), true).Any())
                    {
                        AddParameter(parameters, variable);
                    }

                    assignedVariables.Add(variableName);
                    continue;
                }

                if (!assignedVariables.Contains(variableName))
                {
                    AddParameter(parameters, variable);
                }
            }

            return parameters;
        }

        private static void AddParameter(List<ParameterInfo> parameters, VariableExpressionAst variable)
        {
            if (parameters.Any(m => m.Name.Equals(variable.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            parameters.Add(new ParameterInfo
            {
                Name = variable.VariablePath.UserPath,
                Type = GetParameterType(variable)
            });
        }

        private static string GetParameterType(VariableExpressionAst variable)
        {
            var convertExpression = variable.FindParent<ConvertExpressionAst>();
            if (convertExpression != null && convertExpression.Extent.StartOffset <= variable.Extent.StartOffset && convertExpression.Extent.EndOffset >= variable.Extent.EndOffset)
            {
                return $"[{convertExpression.Type.TypeName.FullName}]";
            }

            var binaryExpression = variable.FindParent<BinaryExpressionAst>();
            if (binaryExpression != null &&
                (binaryExpression.Left is StringConstantExpressionAst || binaryExpression.Right is StringConstantExpressionAst))
            {
                return "[string]";
            }

            var command = variable.FindParent<CommandAst>();
            if (command != null)
            {
                for (int i = 0; i < command.CommandElements.Count - 1; i++)
                {
                    if (command.CommandElements[i] is CommandParameterAst parameter &&
                        command.CommandElements[i + 1].Extent.StartOffset == variable.Extent.StartOffset &&
                        (parameter.ParameterName.Equals("Path", StringComparison.OrdinalIgnoreCase) ||
                         parameter.ParameterName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase)))
                    {
                        return "[string]";
                    }
                }
            }

            return string.Empty;
        }

        private static int GetLineStartOffset(string content, int offset)
        {
            var lineStart = content.LastIndexOf('\n', Math.Max(0, offset - 1));
            return lineStart == -1 ? 0 : lineStart + 1;
        }

        private static IEnumerable<string> NormalizeIndent(string text, string baseIndent)
        {
            return text.Replace("\r\n", "\n").Split('\n')
                .Select(line => line.StartsWith(baseIndent) ? line.Substring(baseIndent.Length) : line);
        }

        private class ParameterInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}
