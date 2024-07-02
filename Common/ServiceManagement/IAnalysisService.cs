using Common.Analysis;
using System;
using System.Collections.Generic;

namespace Common.ServiceManagement
{
    public interface IAnalysisService
    {
        AnalysisRules GetAnalysisRules();
        string RequestFileAnalysis(string fileName);
        string RequestStringAnalysis(string fileName, string content);
        bool IsAnalyzerInstalled();
        bool InstallScriptAnalyzer(out string error);
        string GetScriptAnalyzerVersion();
        event EventHandler<AnalysisResult> OnAnalysisResults;
        IEnumerable<FunctionDefinition> FunctionDefinitions { get; }
        IEnumerable<ClassDefinition> ClassDefinitions { get; }
        void LogAnalysisResults();
    }
}
