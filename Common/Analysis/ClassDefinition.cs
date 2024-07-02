using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Analysis
{
    public class ClassDefinition : IDefinition
    {
        public string Name { get; set; }
        public int Line { get; set; }
        public string FileName { get; set; }

        public List<MemberDefinition> Members { get; set; } = new List<MemberDefinition>();
    }

    public class MemberDefinition : IDefinition
    {
        public string Name { get; set; }
        public int Line { get; set; }
        public string FileName { get; set; }
    }

    public interface IDefinition
    {
        string Name { get; set; }
        int Line { get; set; }
        string FileName { get; set; }
    }
}
