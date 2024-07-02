using Newtonsoft.Json;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation.Language;
using System.Windows.Forms;

namespace IMS.FormDesigner
{
    public class PowerShellEventBindingService : EventBindingService
    {
        private string fileName;

        public PowerShellEventBindingService(IServiceProvider provider, string fileName) : base(provider)
        {
            this.fileName = fileName;
        }

        protected override string CreateUniqueMethodName(IComponent component, EventDescriptor e)
        {
            var control = component as Control;
            if (control == null)
            {
                return component.Site.Name + "_" + e.DisplayName;
            }

            return control.Name + "_" + e.DisplayName;
        }

        protected override void ValidateMethodName(string methodName)
        {
            if (!char.IsDigit(methodName[0]) && methodName.All(m => char.IsLetterOrDigit(m) || m == '_'))
            {
                
            }
            else
            {
                throw new Exception("Invalid method name");
            }
        }

        protected override ICollection GetCompatibleMethods(EventDescriptor e)
        {
            var ast = Parser.ParseFile(fileName, out Token[] tokens, out ParseError[] errors);
            var assignments = ast.FindAll(m => m is AssignmentStatementAst asa && asa.Left is VariableExpressionAst, true)
                .Cast<AssignmentStatementAst>()
                .Select(m => ((VariableExpressionAst)m.Left).VariablePath.UserPath);

            return new ArrayList(assignments.ToArray());
        }

        protected override bool ShowCode()
        {
            throw new NotImplementedException();
        }

        protected override bool ShowCode(int lineNumber)
        {
            throw new NotImplementedException();
        }

        protected override bool ShowCode(IComponent component, EventDescriptor e, string methodName)
        {
            var ast = Parser.ParseFile(fileName, out Token[] tokens, out ParseError[] errors);
            var assignment = ast.Find(m => m is AssignmentStatementAst asa && asa.Left is VariableExpressionAst vea && vea.VariablePath.UserPath.Equals(methodName, StringComparison.OrdinalIgnoreCase), true);

            if (assignment == null)
            {
                var contents = File.ReadAllText(fileName);
                contents = $"${methodName} = {{{Environment.NewLine}}}{Environment.NewLine}{contents}";
                File.WriteAllText(fileName, contents);
            }

            return true;
        }
    }
}
