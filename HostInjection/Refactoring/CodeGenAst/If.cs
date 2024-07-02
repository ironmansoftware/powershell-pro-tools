using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class If : ICodeGenAst
    {
        public ICodeGenAst Condition { get; set; }
        public List<ICodeGenAst> Body { get; set; } = new List<ICodeGenAst>();

        public void Generate(ICodeBuilder stringBuilder)
        {
            stringBuilder.Append("if (");
            Condition.Generate(stringBuilder);
            stringBuilder.Append(")", false);
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("{");
            stringBuilder.Indent();
            foreach(var item in Body)
            {
                item.Generate(stringBuilder);
            }
            stringBuilder.Outdent();
            stringBuilder.AppendLine("}");
        }
    }
}
