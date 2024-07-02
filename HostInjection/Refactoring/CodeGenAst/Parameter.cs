using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host.Refactoring.CodeGenAst
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Position { get; set; }
        public bool Mandatory { get; set; }
        public bool ValueFromPipeline { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Parameter parameter)
            {
                return parameter.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
