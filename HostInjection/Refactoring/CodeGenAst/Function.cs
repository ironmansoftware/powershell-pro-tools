using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class Function : ICodeGenAst
    {
        public string Name { get; set; }
        public ParamBlock ParamBlock { get; set; }
        public List<ICodeGenAst> Body { get; set; } = new List<ICodeGenAst>();

        public void Generate(ICodeBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"function {Name}");
            stringBuilder.AppendLine("{");
            stringBuilder.Indent();

            if (ParamBlock != null)
            {
                ParamBlock.Generate(stringBuilder);
                if (ParamBlock.ParameterSets.SelectMany(m => m.Parameters).Any(m => m.ValueFromPipeline))
                {
                    stringBuilder.AppendLine("Process");
                    stringBuilder.AppendLine("{");
                }
            }
                
            foreach(var item in Body)
            {
                item.Generate(stringBuilder);
            }

            if (ParamBlock != null)
            {
                if (ParamBlock.ParameterSets.SelectMany(m => m.Parameters).Any(m => m.ValueFromPipeline))
                {
                    stringBuilder.AppendLine("}");
                }
            }

            stringBuilder.Outdent();
            stringBuilder.AppendLine("}");
        }
    }
}
