using PowerShellProTools.Host.Refactoring.CodeGenAst;
using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class WrapDotNetMethod : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Wrap .NET method call", RefactorType.ConvertToMultiline);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return ast.GetAstUnderCursor<InvokeMemberExpressionAst>(state) != null;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var statement = ast.GetAstUnderCursor<InvokeMemberExpressionAst>(state);

            if (statement == null)
            {
                yield break;
            }

            var startArguments = statement.ToString().LastIndexOf("(");
            var method = statement.ToString().Substring(0, startArguments);
            var overloads = state.Server.ExecutePowerShellMainRunspace<string>($"{method}.OverloadDefinitions");

            var function = new Function();
            function.Name = method;

            var parameterSets = new List<ParameterSet>();
            var conditions = new List<Condition>();

            int set = 0;
            foreach(var overload in overloads)
            {
                var parameterSet = new ParameterSet();
                parameterSet.Name = $"ParameterSet{set}";

                var argStart = overload.LastIndexOf("(");
                var arguments = overload.Substring(argStart, overload.Length - argStart - 1).Split(',');

                var methodInvoke = new MethodInvoke();
                methodInvoke.Method = method;
                methodInvoke.Static = true;

                foreach (var argument in arguments)
                {
                    var parameter = new Parameter();
                    parameter.Type = argument.Split(' ')[0];
                    parameter.Name = argument.Split(' ')[1];
                    parameter.Mandatory = true;
                    parameterSet.Parameters.Add(parameter);
                }

                var condition = new Condition();
                condition.Left = new RawText { Text = "$PSCmdlet.ParameterSetName" };
                condition.Operator = "-eq";
                condition.Right = new RawText { Text = parameterSet.Name };

                var ifStatement = new If();
                ifStatement.Condition = condition;

                set++;
            }


        }
    }
}
