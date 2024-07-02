using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class ParameterSet
    {
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}
