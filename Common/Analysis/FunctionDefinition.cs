using System.Collections.Generic;
using System.Management.Automation;

namespace Common.Analysis
{
    public class FunctionDefinition : IDefinition
    {
        public string Name { get; set; }
        public string ClassName { get; set;  }
        public int Line { get; set; }
        public string FileName { get; set; }
        public List<ParameterDefinition> Parameters { get; set; }
    }

    public class ParameterDefinition : IDefinition
    {
        public string Name { get; set; }
        public int Line { get; set; }
        public string FileName { get; set; }
    }

}
