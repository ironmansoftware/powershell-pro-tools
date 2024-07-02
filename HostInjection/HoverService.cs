using HostInjection;
using HostInjection.Models;
using PowerShellProTools.Host.Refactoring;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;

namespace PowerShellProTools.Host
{
    public class HoverService
    {
        private readonly IPoshToolsServer _server;

        private DateTime _lastVariables;
        private IEnumerable<Variable> _variables;
        private static object Locker = new object();

        public HoverService(IPoshToolsServer server)
        {
            _server = server;
        }

        public Hover GetHover(TextEditorState state)
        {
            var ast = Parser.ParseInput(state.Content, out Token[] tokens, out ParseError[] errors);
            var variable = ast.GetAstUnderCursor<VariableExpressionAst>(state);
            var member = ast.GetAstUnderCursor<MemberExpressionAst>(state);
            var hoveredAst = ast.GetAstUnderCursor(state);

            var markdown = string.Empty;
            if (variable != null)
            {
                if (DateTime.Now - _lastVariables > TimeSpan.FromSeconds(3))
                {
                    lock (Locker)
                    {
                        _variables = _server.GetVariables(false).ToArray();
                        _lastVariables = DateTime.Now;
                    }
                }

                if (_variables != null)
                {
                    var variableValue = _variables.FirstOrDefault(m => m.VarName.Equals(variable.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase));

                    if (variableValue != null)
                    {
                        markdown = $"**Value:** {variableValue.VarValue}/n**Type:** {variableValue.Type}/n";
                    }
                }
            }

            if (member?.Expression is TypeExpressionAst typeExpression && member.Member is StringConstantExpressionAst memberName)
            {
                var type = FindType(typeExpression.TypeName.FullName);
                if (type == null)
                {
                    type = FindType("System." + typeExpression.TypeName.FullName);
                }

                if (type != null)
                {
                    var memberInfo = type.GetMembers().Where(m =>  m.Name.Equals(memberName.Value, StringComparison.OrdinalIgnoreCase)).ToArray();

                    foreach (var methodInfo in memberInfo.OfType<MethodInfo>())
                    {
                        var parameters = methodInfo.GetParameters();
                        var paramString = "";
                        if (parameters.Any())
                        {
                            paramString= methodInfo.GetParameters().Select(m => $"{m.ParameterType.Name} {m.Name}")
                                .Aggregate((x, y) => x + "," + y);
                        }

                        markdown = $"**Signature:** {methodInfo.ReturnType.Name} {methodInfo.Name}({paramString})/n";
                    }

                    var propertyInfo = memberInfo.OfType<PropertyInfo>().FirstOrDefault();
                    if (propertyInfo != null)
                    {
                        markdown = $"**Type:** {propertyInfo.PropertyType.FullName}/n";
                    }
                }
            }

            if (member?.Expression is VariableExpressionAst variableExpression && member.Member is StringConstantExpressionAst memberName2 && _variables != null)
            {
                var variableValue = _variables.FirstOrDefault(m =>
                    m.VarName.Equals(variableExpression.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase));

                if (variableValue != null)
                {
                    var type = FindType(variableValue.Type);
                    if (type == null)
                    {
                        type = FindType("System." + variableValue.Type);
                    }

                    var memberInfo = type.GetMembers().Where(m => m.Name.Equals(memberName2.Value, StringComparison.OrdinalIgnoreCase)).ToArray();

                    foreach (var methodInfo in memberInfo.OfType<MethodInfo>())
                    {
                        var parameters = methodInfo.GetParameters();
                        var paramString = "";
                        if (parameters.Any())
                        {
                            paramString = methodInfo.GetParameters().Select(m => $"{m.ParameterType.Name} {m.Name}")
                                .Aggregate((x, y) => x + "," + y);
                        }

                        markdown += $"{methodInfo.ReturnType.Name} **{methodInfo.Name}**({paramString})/n";
                    }

                    var propertyInfo = memberInfo.OfType<PropertyInfo>().FirstOrDefault();
                    if (propertyInfo != null)
                    {
                        markdown = $"**Type:** {propertyInfo.PropertyType.FullName}/n";
                    }
                }

            }


            if (hoveredAst != null)
                markdown += $"**AST Type:** {hoveredAst.GetType().Name}";

            return new Hover
            {
                Markdown = markdown
            };
        }

        /// <summary>
        /// Looks in all loaded assemblies for the given type.
        /// </summary>
        /// <param name="fullName">
        /// The full name of the type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/> found; null if not found.
        /// </returns>
        private static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName?.Equals(fullName, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
