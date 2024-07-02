using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class GenerateProxyFunction : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Generate proxy function", RefactorType.GenerateProxyFunction);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return ast.GetAstUnderCursor<CommandAst>(state) != null;
        }

        public  IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var edits = new List<TextEdit>();
            var command = ast.GetAstUnderCursor<CommandAst>(state);
            if (command == null || string.IsNullOrEmpty(command.GetCommandName()))
            {
                return edits;
            }

            var proxy = state.Server.ExecutePowerShellMainRunspace<string>($"$Commands = Get-Command '{command.GetCommandName()}' -ErrorAction SilentlyContinue; if ($Commands) {{ try {{ [System.Management.Automation.ProxyCommand]::Create((New-Object System.Management.Automation.CommandMetaData $Commands)) }} catch {{ }} }} else {{ '' }} ").FirstOrDefault();

            if (string.IsNullOrEmpty(proxy))
            {
                return edits;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"function {command.GetCommandName()}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(string.Join(Environment.NewLine, proxy.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(m => "\t" + m)));
            stringBuilder.AppendLine("}");

            var line = command.Extent.StartLineNumber - 2;

            edits.Add(new TextEdit
            {
                Content = stringBuilder.ToString(),
                Type = TextEditType.Insert,
                Start = new TextPosition
                {
                    Line = line < 0 ? 0 : line,
                    Character = 0,
                    Index = 0
                },
                FileName = state.FileName
            });
            return edits;
        }
    }
}
