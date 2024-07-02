using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class MethodInvoke : ICodeGenAst
    {
        public ICodeGenAst Object { get; set; }
        public bool Static { get; set; }
        public string Method { get; set; }
        public List<ICodeGenAst> Arguments { get; set; } = new List<ICodeGenAst>();
        public void Generate(ICodeBuilder stringBuilder)
        {
            Object.Generate(stringBuilder);
            if (Static)
            {
                stringBuilder.Append("::", false);
            }
            else
            {
                stringBuilder.Append(".", false);
            }
            stringBuilder.Append(Method + "(", false);
            for(int i = 0; i < Arguments.Count; i++)
            {
                Arguments[i].Generate(stringBuilder);
                if (i < Arguments.Count - 2)
                {
                    stringBuilder.Append(",", false);
                }
            }
            stringBuilder.Append(")");
        }
    }
}
