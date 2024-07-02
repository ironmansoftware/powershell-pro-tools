using Common.Analysis;
using Microsoft.VisualStudio.PlatformUI;
using ModernWpf;
using Newtonsoft.Json;
using PowerShellProTools.Host;
using PowerShellTools.Common.Logging;
using PowerShellTools.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PowerShellTools.Shared.ToolWindows
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel(bool analyzer = true)
        {
            AnalyzerSettings = new AnalyzerSettingsViewModel();
            if (analyzer)
            {
                AnalyzerVisibility = Visibility.Visible;
            }
            else
            {
                AnalyzerVisibility = Visibility.Hidden;
            }
        }


        private Visibility _fullSettingsVisibility;

        public Visibility FullSettingsVisibility
        {
            get { return _fullSettingsVisibility; }
            set { _fullSettingsVisibility = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullSettingsVisibility))); }
        }


        public Visibility AnalyzerVisibility
        {
            get;set;
        }

        private bool _dontShowOnStartup;
        public bool DontShowOnStartup
        {
            get
            {
                return _dontShowOnStartup;
            }
            set
            {
                _dontShowOnStartup = value;
                GeneralOptions.Instance.DontShowLicenseInfoOnStartup = value;
                GeneralOptions.Instance.Save();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DontShowOnStartup)));
            }
        }

        public AnalyzerSettingsViewModel AnalyzerSettings { get; }
    }

    public class AnalyzerSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly string AnaylsisSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Pro Tools", "AnalysisSettings.json");
        private readonly string ScriptAnalyzerSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Pro Tools", "PSScriptAnalyzerSettings.psd1");
        private AnalyzerSettings AnalyzerSettings = new AnalyzerSettings();
        private static readonly ILog Log = LogManager.GetLogger(typeof(AnalyzerSettingsViewModel));
        private AnalysisRule[] rules;

        public AnalyzerSettingsViewModel()
        {
            Load();
        }

        public void Load()
        {
            if (PowerShellToolsPackage.Instance?.AnalysisService == null)
            {
                return;
            }

            var proToolsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Pro Tools");

            if (!Directory.Exists(proToolsDir))
            {
                Directory.CreateDirectory(proToolsDir);
            }

            if (!PowerShellToolsPackage.Instance.AnalysisService.IsAnalyzerInstalled())
            {
                var dialogResult = System.Windows.Forms.MessageBox.Show("PSScriptAnalyzer is not installed. It is requried for script analysis. Would you like to install PSScriptAnalyzer?", "PSScriptAnalyzer Not Installed", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    Status = "Installing PSScriptAnalyzer...";
                    if (!PowerShellToolsPackage.Instance.AnalysisService.InstallScriptAnalyzer(out string error))
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to install PSScriptAnalyzer. {error}", "Failed", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Status = "PSScriptAnalyzer Version: " + PowerShellToolsPackage.Instance.AnalysisService.GetScriptAnalyzerVersion();

            rules = (PowerShellToolsPackage.Instance.AnalysisService.GetAnalysisRules()).Rules.ToArray();
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (rules == null)
            {
                rules = Array.Empty<AnalysisRule>();
            }

            if (!File.Exists(AnaylsisSettingsPath))
            {
                AnalyzerSettings.EnabledLevels = new List<string> { "Error", "Information", "Warning", "ParseError" };
                AnalyzerSettings.EnabledRules = rules.Select(m => m.Name).ToList();
                SaveSettings();
            }
            else
            {
                var json = File.ReadAllText(AnaylsisSettingsPath);
                AnalyzerSettings = JsonConvert.DeserializeObject<AnalyzerSettings>(json);
            }

            var levels = new[]
            {
                new AnalysisRuleViewModel { Name = "Error", Enabled = AnalyzerSettings.EnabledLevels.Contains("Error") },
                new AnalysisRuleViewModel { Name = "Information", Enabled = AnalyzerSettings.EnabledLevels.Contains("Information") },
                new AnalysisRuleViewModel { Name = "Warning", Enabled = AnalyzerSettings.EnabledLevels.Contains("Warning") },
                new AnalysisRuleViewModel { Name = "ParseError", Enabled = AnalyzerSettings.EnabledLevels.Contains("ParseError") },
            };

            Levels = new ObservableCollection<AnalysisRuleViewModel>(levels);
            Enabled = AnalysisOptions.Instance.ScriptAnalyzer;
            SolutionWideAnalysis = AnalysisOptions.Instance.SolutionWideAnalysis;
            AnalyzeOnSave = AnalysisOptions.Instance.AnalyzeOnSave;

            Rules = new ObservableCollection<AnalysisRuleViewModel>(rules.Select(m => new AnalysisRuleViewModel
            {
                Name = m.Name,
                Description = m.Description,
                Enabled = AnalyzerSettings.EnabledRules.Contains(m.Name)
            }).ToList());
        }

        public void FilterRules(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Rules = new ObservableCollection<AnalysisRuleViewModel>(rules.Select(m => new AnalysisRuleViewModel
                {
                    Name = m.Name,
                    Description = m.Description,
                    Enabled = AnalyzerSettings.EnabledRules.Contains(m.Name)
                }).ToList());
            }
            else
            {
                Rules = new ObservableCollection<AnalysisRuleViewModel>(rules.Where(m => m.Name.ToLower().Contains(text.ToLower())).Select(m => new AnalysisRuleViewModel
                {
                    Name = m.Name,
                    Description = m.Description,
                    Enabled = AnalyzerSettings.EnabledRules.Contains(m.Name)
                }).ToList());
            }

        }

        public void SaveSettings()
        {
            try
            {
                AnalysisOptions.Instance.SolutionWideAnalysis = SolutionWideAnalysis;
                AnalysisOptions.Instance.ScriptAnalyzer = Enabled;
                AnalysisOptions.Instance.AnalyzeOnSave = AnalyzeOnSave;
                AnalysisOptions.Instance.Save();
                AnalysisOptions.Instance.OnScriptAnalyzerChanged();
                AnalysisOptions.Instance.OnSolutionWideAnalysisChanged();
                AnalysisOptions.Instance.OnAnalyzeOnSaveChanged();

                AnalyzerSettings.EnabledRules = Rules.Where(m => m.Enabled).Select(m => m.Name).ToList();
                var json = JsonConvert.SerializeObject(AnalyzerSettings);
                File.WriteAllText(AnaylsisSettingsPath, json);

                var excludedRules = rules.Where(m => !AnalyzerSettings.EnabledRules.Contains(m.Name));

                string excludedRulesString = string.Empty;
                if (excludedRules.Any())
                {
                    excludedRulesString = excludedRules.Select(m => $"'{m.Name}'").Aggregate((x, y) => x + "," + y);
                }

                string enabledLevelsString = string.Empty;
                if (Levels.Any(m => m.Enabled))
                {
                    AnalyzerSettings.EnabledLevels = Levels.Where(m => m.Enabled).Select(m => m.Name).ToList();
                    enabledLevelsString = AnalyzerSettings.EnabledLevels.Select(x => $"'{x}'").Aggregate((x, y) => x + "," + y);
                }

                var script = $@"@{{ 
    Severity = @({enabledLevelsString})
    ExcludeRules = @({excludedRulesString})
}}";

                File.WriteAllText(ScriptAnalyzerSettings, script);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save settings", ex);
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public bool AllSelected
        {
            get
            {
                return Rules.All(m => m.Enabled);
            }
            set
            {
                foreach(var rule in Rules)
                {
                    rule.Enabled = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllSelected)));
            }
        }

        public bool Enabled { get; set; }
        public bool SolutionWideAnalysis { get; set; }
        public bool AnalyzeOnSave { get; set; }

        private ObservableCollection<AnalysisRuleViewModel> _rules;
        public ObservableCollection<AnalysisRuleViewModel> Rules
        {
            get
            {
                return _rules;
            }
            set
            {
                _rules = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rules)));
            }
        }

        public ObservableCollection<AnalysisRuleViewModel> _levels;
        public ObservableCollection<AnalysisRuleViewModel> Levels
        {
            get
            {
                return _levels;
            }
            set
            {
                _levels = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Levels)));
            }
        }
    }

    internal class AnalyzerSettings
    {
        public List<string> EnabledLevels { get; set; }
        public List<string> EnabledRules { get; set; }
    }

    public class AnalysisRuleViewModel
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
