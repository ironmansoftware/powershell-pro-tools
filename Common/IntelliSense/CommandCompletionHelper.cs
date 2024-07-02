using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace PowerShellTools.Common.IntelliSense
{
    public static class CommandCompletionHelper
    {
        public static object _lock = new object();
        private static PowerShell _currentPowerShell;

        /// <summary>
        /// Completion lists calculator.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="caretPosition">The caret position.</param>
        /// <param name="runspace">Runspace for completion computing.</param>
        /// <returns></returns>
        public static CommandCompletion GetCommandCompletionList(string script, int caretPosition, Runspace runspace)
        {
            Ast ast;
            Token[] tokens;
            IScriptPosition cursorPosition;
            GetCommandCompletionParameters(script, caretPosition, out ast, out tokens, out cursorPosition);
            if (ast == null)
            {
                return null;
            }

            CommandCompletion commandCompletion = null;

            if (runspace.RunspaceAvailability == RunspaceAvailability.Available)
            {
                using (_currentPowerShell = PowerShell.Create())
                {
                    _currentPowerShell.Runspace = runspace;
                    commandCompletion = CommandCompletion.CompleteInput(ast, tokens, cursorPosition, null, _currentPowerShell);
                }
            }

            return commandCompletion;
        }

        /// <summary>
        /// Dismiss the current running completion request
        /// </summary>
        public static void DismissCommandCompletionListRequest()
        {
            lock (_lock)
            {
                if (_currentPowerShell != null)
                {
                    _currentPowerShell.Stop();
                }
            }
        }

        /// <summary>
        /// Get the abstract syntax tree, tokens and the cursor position.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="caretPosition">The caret position.</param>
        /// <param name="ast">The AST to get.</param>
        /// <param name="tokens">The tokens to get.</param>
        /// <param name="cursorPosition">The cursor position to get.</param>
        public static void GetCommandCompletionParameters(string script, int caretPosition, out Ast ast, out Token[] tokens, out IScriptPosition cursorPosition)
        {       
            ParseError[] array;            
            ast = Tokenize(script, out tokens, out array);            
            if (ast != null)
            {
                //HACK: Clone with a new offset using private method... 
                var type = ast.Extent.StartScriptPosition.GetType();
                var method = type.GetMethod("CloneWithNewOffset", 
                                            BindingFlags.Instance | BindingFlags.NonPublic, 
                                            null,
                                            new[] { typeof(int) }, null);

                cursorPosition = (IScriptPosition)method.Invoke(ast.Extent.StartScriptPosition, new object[] { caretPosition });
                return;
            }
            cursorPosition = null;
        }

        /// <summary>
        /// Tokonize the script and get the needed data.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="tokens">The tokens to get.</param>
        /// <param name="errors">The parse errors to get.</param>
        /// <returns></returns>
        public static Ast Tokenize(string script, out Token[] tokens, out ParseError[] errors)
        {
            Ast result;
            try
            {
                Ast ast = Parser.ParseInput(script, out tokens, out errors);
                result = ast;
            }
            catch (RuntimeException ex)
            {
                var parseError = new ParseError(new EmptyScriptExtent(), ex.ErrorRecord.FullyQualifiedErrorId, ex.Message);
                errors = new[] { parseError };
                tokens = new Token[0];
                result = null;
            }
            return result;
        }
    }
}
