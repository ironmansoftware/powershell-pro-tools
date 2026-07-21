using PowerShellProTools.Host;
using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Common.Refactoring
{
    public class RenameSymbol 
    {
        public IEnumerable<TextEdit> Refactor(string newName, TextEditorState state, Ast ast, Workspace workspace)
        {
            var results = new List<TextEdit>();

            var functionName = GetFunctionNameUnderCursor(state, ast);
            if (functionName != null)
            {
                var functions = ast.FindAll(m => m is FunctionDefinitionAst function && function.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase), true).Cast<FunctionDefinitionAst>().ToArray();
                if (functions.Any())
                {
                    foreach (var function in functions)
                    {
                        var definitionEdit = CreateFunctionDefinitionEdit(newName, state.FileName, function);
                        if (definitionEdit != null)
                        {
                            results.Add(definitionEdit);
                        }
                    }

                    var commands = ast.FindAll(m => m is CommandAst command && command.GetCommandName()?.Equals(functionName, StringComparison.OrdinalIgnoreCase) == true, true).Cast<CommandAst>();
                    foreach (var command in commands)
                    {
                        var commandName = command.CommandElements.FirstOrDefault();
                        if (commandName == null)
                        {
                            continue;
                        }

                        results.Add(new TextEdit
                        {
                            Content = newName,
                            FileName = state.FileName,
                            Type = TextEditType.Replace,
                            Start = new TextPosition
                            {
                                Character = commandName.Extent.StartColumnNumber - 1,
                                Line = commandName.Extent.StartLineNumber - 1,
                                Index = commandName.Extent.StartOffset
                            },
                            End = new TextPosition
                            {
                                Character = commandName.Extent.EndColumnNumber - 1,
                                Line = commandName.Extent.EndLineNumber - 1,
                                Index = commandName.Extent.EndOffset
                            }
                        });
                    }

                    return results.Distinct();
                }
            }

            var variable = ast.GetAstUnderCursor<VariableExpressionAst>(state);
            if (variable != null)
            {
                if (variable.IsNestedInFunction())
                {
                    var parentFunction = variable.FindParent<FunctionDefinitionAst>();
                    var functionInfo = workspace.FunctionDefinitions.FirstOrDefault(m => m.FileName.Equals(state.FileName, StringComparison.OrdinalIgnoreCase) && m.Name.Equals(parentFunction.Name, StringComparison.OrdinalIgnoreCase));

                    if (functionInfo == null) return results;

                    foreach(var funcVariable in functionInfo.Variables.Where(m => m.VariablePath.UserPath.Equals(variable.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        results.Add(new TextEdit
                        {
                            Content = newName,
                            FileName = state.FileName,
                            Type = TextEditType.Replace,
                            Start = new TextPosition
                            {
                                Character = funcVariable.Extent.StartColumnNumber - 1,
                                Line = funcVariable.Extent.StartLineNumber - 1
                            },
                            End = new TextPosition
                            {
                                Character = funcVariable.Extent.EndColumnNumber - 1,
                                Line = funcVariable.Extent.EndLineNumber - 1
                            },
                        });
                    }
                }
                else
                {
                    var scriptBlockAst = ast as ScriptBlockAst;
                    if (scriptBlockAst != null)
                    {
                        var variables = scriptBlockAst.EndBlock.FindAll(m => m is VariableExpressionAst expressionAst && expressionAst.VariablePath.UserPath.Equals(variable.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase), false).Cast<VariableExpressionAst>();
                        foreach(var v1 in variables)
                        {
                            results.Add(new TextEdit
                            {
                                Content = newName,
                                FileName = state.FileName,
                                Type = TextEditType.Replace,
                                Start = new TextPosition
                                {
                                    Character = v1.Extent.StartColumnNumber - 1,
                                    Line = v1.Extent.StartLineNumber - 1,  
                                    Index = v1.Extent.StartOffset
                                },
                                End = new TextPosition
                                {
                                    Character = v1.Extent.EndColumnNumber - 1,
                                    Line = v1.Extent.EndLineNumber - 1,
                                    Index = v1.Extent.EndOffset
                                },
                            });
                        }
                    }

                    foreach (var globalVar in workspace.Variables.Where(m => m.Ast.VariablePath.UserPath.Equals(variable.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        results.Add(new TextEdit
                        {
                            Content = newName,
                            FileName = globalVar.FileName,
                            Type = TextEditType.Replace,
                            Start = new TextPosition
                            {
                                Character = globalVar.Ast.Extent.StartColumnNumber - 1,
                                Line = globalVar.Ast.Extent.StartLineNumber - 1,
                                Index = globalVar.Ast.Extent.StartOffset
                            },
                            End = new TextPosition
                            {
                                Character = globalVar.Ast.Extent.EndColumnNumber - 1,
                                Line = globalVar.Ast.Extent.EndLineNumber - 1,
                                Index = globalVar.Ast.Extent.EndOffset
                            },
                        });
                    }
                }
            }

            return results.Distinct();
        }

        private static string GetFunctionNameUnderCursor(TextEditorState state, Ast ast)
        {
            var function = ast.FindAll(m => m is FunctionDefinitionAst, true)
                .Cast<FunctionDefinitionAst>()
                .FirstOrDefault(m =>
                {
                    var edit = CreateFunctionDefinitionEdit(m.Name, state.FileName, m);
                    return edit != null && ContainsPosition(edit.Start, edit.End, state.SelectionStart);
                });

            if (function != null)
            {
                return function.Name;
            }

            var command = ast.FindAll(m => m is CommandAst, true)
                .Cast<CommandAst>()
                .FirstOrDefault(m =>
                {
                    var commandName = m.CommandElements.FirstOrDefault();
                    return commandName != null && ContainsPosition(commandName.Extent, state.SelectionStart);
                });

            return command?.GetCommandName();
        }

        private static TextEdit CreateFunctionDefinitionEdit(string newName, string fileName, FunctionDefinitionAst function)
        {
            var firstLine = function.Extent.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).FirstOrDefault();
            if (firstLine == null)
            {
                return null;
            }

            var nameIndex = firstLine.IndexOf(function.Name, StringComparison.OrdinalIgnoreCase);
            if (nameIndex < 0)
            {
                return null;
            }

            var startCharacter = function.Extent.StartColumnNumber - 1 + nameIndex;

            return new TextEdit
            {
                Content = newName,
                FileName = fileName,
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Character = startCharacter,
                    Line = function.Extent.StartLineNumber - 1,
                    Index = function.Extent.StartOffset + nameIndex
                },
                End = new TextPosition
                {
                    Character = startCharacter + function.Name.Length,
                    Line = function.Extent.StartLineNumber - 1,
                    Index = function.Extent.StartOffset + nameIndex + function.Name.Length
                }
            };
        }

        private static bool ContainsPosition(IScriptExtent extent, TextPosition position)
        {
            return extent.StartLineNumber <= position.Line + 1 &&
                extent.EndLineNumber >= position.Line + 1 &&
                extent.StartColumnNumber <= position.Character + 1 &&
                extent.EndColumnNumber >= position.Character + 1;
        }

        private static bool ContainsPosition(TextPosition start, TextPosition end, TextPosition position)
        {
            return start.Line <= position.Line &&
                end.Line >= position.Line &&
                start.Character <= position.Character &&
                end.Character >= position.Character;
        }
    }
}
