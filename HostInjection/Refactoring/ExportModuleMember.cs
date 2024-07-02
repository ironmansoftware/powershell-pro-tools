using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class ExportModuleMember : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Export module member", RefactorType.ExportModuleMember);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                (m is VariableExpressionAst ||
                m is FunctionDefinitionAst), true);

            return statement != null;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                (m is VariableExpressionAst ||
                 m is FunctionDefinitionAst), true);

            if (statement == null)
            {
                yield break;
            }

            if (statement is VariableExpressionAst variableExpression)
            {
                yield return new TextEdit
                {
                    Type = TextEditType.Insert,
                    Content = $"{Environment.NewLine}Export-ModuleMember -Variable '{variableExpression.VariablePath.UserPath}'",
                    Start = new TextPosition
                    {
                        Character = 0,
                        Line = state.Content.Split(Environment.NewLine.ToCharArray()).Length + 1
                    },
                    FileName = state.FileName
                };
            }
            else if (statement is FunctionDefinitionAst functionDef)
            {
                yield return new TextEdit
                {
                    Type = TextEditType.Insert,
                    Content = $"{Environment.NewLine}Export-ModuleMember -Function '{functionDef.Name}'",
                    Start = new TextPosition
                    {
                        Character = 0,
                        Line = state.Content.Split(Environment.NewLine.ToCharArray()).Length,
                        Index = 0
                    },
                    FileName = state.FileName
                };
            }
        }
    }
}
