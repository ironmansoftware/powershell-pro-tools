using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Analysis
{
    public class AnalysisResult
    {
        public string FileName { get; set; }
        public string RequestId { get; set; }
        public DateTime LastModified { get; set; }
        public List<AnalysisIssue> Issues { get; set; } = new List<AnalysisIssue>();
        public List<FunctionDefinition> FunctionDefinitions { get; set; } = new List<FunctionDefinition>();
        public List<ClassDefinition> ClassDefinitions { get; set; } = new List<ClassDefinition>();
    }
}
