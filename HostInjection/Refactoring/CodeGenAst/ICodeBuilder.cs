using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public interface ICodeBuilder
    {
        void Indent();
        void Outdent();
        void AppendLine(string str);
        void Append(string str, bool indepent = true);
    }
}
