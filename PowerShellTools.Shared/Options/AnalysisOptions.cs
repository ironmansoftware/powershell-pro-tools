using PowerShellTools.DebugEngine;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PowerShellTools.Options
{
    internal class AnalysisOptions : BaseOptionModel<AnalysisOptions>
    {
        public event EventHandler<EventArgs<bool>> ScriptAnalyzerChanged;
        public event EventHandler<EventArgs<bool>> SolutionWideAnalysisChanged;
        public event EventHandler<EventArgs<bool>> AnalyzeOnSaveChanged;

        public AnalysisOptions()
        {
            this.SolutionWideAnalysis = this.ScriptAnalyzer;
        }

        [DisplayName(@"Script Analyzer")]
        [Description("When true, enables script analyzer support.")]
        public bool ScriptAnalyzer { get; set; }

        [DisplayName(@"Solution Wide Analysis")]
        [Description("When true, enables script analyzer support for the entire solution.")]
        public bool SolutionWideAnalysis { get; set; }

        [DisplayName(@"Analyze on Save")]
        [Description("When true, analyzes a file when it is saved.")]
        public bool AnalyzeOnSave { get; set; }

        public override void Load()
        {
            var previousScriptAnalyzer = ScriptAnalyzer;
            var previousSolutionWideAnalysis = SolutionWideAnalysis;
            var previousAnalyzeOnSave = AnalyzeOnSave;

            base.Load();

            if (previousAnalyzeOnSave != AnalyzeOnSave)
            {
                OnAnalyzeOnSaveChanged();
            }

            if (previousScriptAnalyzer != ScriptAnalyzer)
            {
                OnScriptAnalyzerChanged();
            }

            if (previousSolutionWideAnalysis != SolutionWideAnalysis)
            {
                OnSolutionWideAnalysisChanged();
            }
        }

        public override async Task LoadAsync()
        {
            var previousScriptAnalyzer = ScriptAnalyzer;
            var previousSolutionWideAnalysis = SolutionWideAnalysis;
            var previousAnalyzeOnSave = AnalyzeOnSave;

            await base.LoadAsync();

            if (previousAnalyzeOnSave != AnalyzeOnSave)
            {
                OnAnalyzeOnSaveChanged();
            }

            if (previousScriptAnalyzer != ScriptAnalyzer)
            {
                OnScriptAnalyzerChanged();
            }

            if (previousSolutionWideAnalysis != SolutionWideAnalysis)
            {
                OnSolutionWideAnalysisChanged();
            }
        }

        public void OnAnalyzeOnSaveChanged()
        {
            AnalyzeOnSaveChanged?.Invoke(this, new EventArgs<bool>(AnalyzeOnSave));
        }

        public void OnScriptAnalyzerChanged()
        {
            ScriptAnalyzerChanged?.Invoke(this, new EventArgs<bool>(ScriptAnalyzer));
        }

        public void OnSolutionWideAnalysisChanged()
        {
            SolutionWideAnalysisChanged?.Invoke(this, new EventArgs<bool>(SolutionWideAnalysis));
        }
    }
}
