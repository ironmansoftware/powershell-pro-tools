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

            //var function = ast.GetAstUnderCursor<FunctionDefinitionAst>(state);
            //if (function == null) yield break;

            //var functionInfo = workspace.FunctionDefinitions.FirstOrDefault(m => m.Name.Equals(function.Name, StringComparison.OrdinalIgnoreCase));
            //if (functionInfo == null) yield break;


        }
    }
}
