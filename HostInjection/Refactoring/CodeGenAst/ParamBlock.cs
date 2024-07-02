using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class ParamBlock : ICodeGenAst
    {
        public List<ParameterSet> ParameterSets { get; set; } = new List<ParameterSet>();

        public void Generate(ICodeBuilder codeBuilder)
        {
            codeBuilder.AppendLine("param(");
            var parameters = ParameterSets.SelectMany(m => m.Parameters).Distinct();
            codeBuilder.Indent();
            foreach(var parameter in parameters)
            {
                foreach(var parameterSet in ParameterSets.Where(m => m.Parameters.Contains(parameter)))
                {
                    var parameterSetParameter = parameterSet.Parameters.First(m => m.Name.Equals(parameter.Name));
                    codeBuilder.Append($"[Parameter(ParameterSetName = '{parameterSet.Name}'");
                    if (parameterSetParameter.Mandatory)
                    {
                        codeBuilder.Append(", Mandatory", false);
                    }

                    if (parameterSetParameter.ValueFromPipeline)
                    {
                        codeBuilder.Append(", ValueFromPipeline", false);
                    }

                    codeBuilder.Append(")]", false);
                }

                codeBuilder.AppendLine($"[{parameter.Type}]${parameter.Name}");
            }
            codeBuilder.Outdent();
            codeBuilder.AppendLine(")");
        }
    }
}
