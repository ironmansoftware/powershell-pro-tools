using PowerShellProTools.Host.Refactoring;
using System.Linq;
using System.Management.Automation.Language;

namespace PowerShellProTools.Host
{
    public static class AstExtensions
    {
        public static T GetAstUnderCursor<T>(this Ast ast, TextEditorState state) where T : class
        {
            var statement = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionStart.Character + 1 &&
                m is T, true);

            return statement as T;
        }

        public static Ast GetAstUnderCursor(this Ast ast, TextEditorState state)
        {
            var statement = ast.FindAll(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber <= state.SelectionStart.Character + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionStart.Character + 1, true);

            return statement.LastOrDefault();
        }

        public static T FindParent<T>(this Ast ast) where T : Ast
        {
            if (ast.Parent is T)
            {
                return (T)ast.Parent;
            }

            if (ast.Parent == null)
            {
                return null;
            }

            return FindParent<T>(ast.Parent);
        }

        public static bool IsNestedInFunction(this Ast ast)
        {
            if (ast.Parent is FunctionDefinitionAst)
            {
                return true;
            }

            if (ast.Parent == null)
            {
                return false;
            }

            return IsNestedInFunction(ast.Parent);
        }
    }
}
