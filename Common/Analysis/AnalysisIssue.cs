using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace Common.Analysis
{
    public class AnalysisIssue
    {
        public string Message { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string Severity { get; set; }
        public ScriptExtent Extent { get; set; }
        [JsonProperty("SuggestedCorrections")]
        public List<AnalysisSuggestion> Suggestions { get; set; } = new List<AnalysisSuggestion>();
    }

    public class ScriptExtent
    {
        public int EndColumnNumber { get; set; }

        public int EndLineNumber { get; set; }

        public int EndOffset { get; set; }


        public string File { get; set; }

        public int StartColumnNumber { get; set; }

        public int StartLineNumber { get; set; }

        public int StartOffset { get; set; }

        public string Text { get; set; }
    }
}
