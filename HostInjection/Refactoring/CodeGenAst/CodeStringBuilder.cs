using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class CodeStringBuilder : ICodeBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        private int indepent = 0; 

        public void Append(string str, bool indent = true)
        {
            if (indent)
            {
                for (int i = 0; i < indepent; i++)
                {
                    stringBuilder.Append("\t");
                }
            }
            stringBuilder.Append(str);
        }

        public void AppendLine(string str)
        {
            for (int i = 0; i < indepent; i++)
            {
                stringBuilder.Append("\t");
            }
            stringBuilder.AppendLine(str);
        }

        public void Indent()
        {
            indepent++;
        }

        public void Outdent()
        {
            indepent--;
        }
    }
}
