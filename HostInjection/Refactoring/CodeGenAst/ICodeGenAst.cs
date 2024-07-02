using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public interface ICodeGenAst
    {
        void Generate(ICodeBuilder stringBuilder);
    }
}
