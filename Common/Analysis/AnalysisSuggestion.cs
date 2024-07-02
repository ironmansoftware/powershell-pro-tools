using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Analysis
{
    public class AnalysisSuggestion
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public int StartLineNumber { get; set; }
        public int StartColumnNumber { get; set; }
        public int EndLineNumber { get; set; }
        public int EndColumnNumber { get; set; }
    }
}
