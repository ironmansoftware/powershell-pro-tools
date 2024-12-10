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
        public event EventHandler<EventArgs<string>> ConfigurationFileChanged;

        public AnalysisOptions()
        {
            this.SolutionWideAnalysis = this.ScriptAnalyzer;
        }

        [DisplayName(@"Enable Script Analyzer")]
        [Description("When true, enables PSScriptAnalyzer support.")]
        public bool ScriptAnalyzer { get; set; }

        [DisplayName(@"Solution Wide Analysis")]
        [Description("When true, enables script analyzer support for the entire solution.")]
        public bool SolutionWideAnalysis { get; set; }

        [DisplayName(@"Analyze on Save")]
        [Description("When true, analyzes a file when it is saved. When false, the script will be analyzed as you type.")]
        public bool AnalyzeOnSave { get; set; }

        [DisplayName(@"Configuration File")]
        [Description("The path to a PSScriptAnalyzer configuration file.")]
        public string ConfigurationFile { get; set; }

        public override void Load()
        {
            var previousScriptAnalyzer = ScriptAnalyzer;
            var previousSolutionWideAnalysis = SolutionWideAnalysis;
            var previousAnalyzeOnSave = AnalyzeOnSave;
            var previousConfigurationFile = ConfigurationFile;


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

            if (previousConfigurationFile != ConfigurationFile)
            {
                OnConfigurationFileChanged();
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

        public void OnConfigurationFileChanged()
        {
            ConfigurationFileChanged?.Invoke(this, new EventArgs<string>(ConfigurationFile));
        }
    }
}
