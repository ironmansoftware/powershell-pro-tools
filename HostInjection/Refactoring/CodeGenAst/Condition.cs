using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class Condition : ICodeGenAst
    {
        public ICodeGenAst Left { get; set; }
        public string Operator { get; set; }
        public ICodeGenAst Right { get; set; }

        public void Generate(ICodeBuilder stringBuilder)
        {
            Left.Generate(stringBuilder);
            stringBuilder.Append($" {Operator} ", false);
            Right.Generate(stringBuilder);
        }
    }
}

