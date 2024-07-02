using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudioTools;
using PowerShellTools.Common.Logging;
using PowerShellTools.LanguageService;
using Tasks = System.Threading.Tasks;
using System.Collections.Concurrent;
using PowerShellTools.Options;
using PowerShellTools.Service;
using Common.Analysis;
using PowerShellTools.HostService;

namespace PowerShellTools.Classification
{
    internal class PowerShellTokenizationService : IPowerShellTokenizationService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellTokenizationService));
        private readonly object _tokenizationLock = new object();
        private static List<string> TaskIdentifiers;
        public event EventHandler<Ast> TokenizationComplete;

        private readonly ClassifierService _classifierService;
        private readonly RegionAndBraceMatchingService _regionAndBraceMatchingService;

        private ITextBuffer _textBuffer;
        private ITextSnapshot _lastSnapshot;
        private static TodoWindowTaskProvider taskProvider;
        private static ErrorListProvider errorListProvider;
        private bool scriptAnalyzerEnabled;
        private bool analyzeOnSave;
        private readonly BlockingCollection<ITextSnapshot> _textSnapshots;
        private readonly AnalysisWatcher watcher;

        public static ConcurrentDictionary<string, ITextSnapshot> AnalysisRequests { get; } = new ConcurrentDictionary<string, ITextSnapshot>();

        private static Regex taskListRegex;

        private static void LoadTaskListTokens()
        {
            TaskIdentifiers = new List<string>();
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            TaskList tl = dte.ToolWindows.TaskList;
            dynamic commentTokens = ((dynamic)tl).CommentTokens;

            // getting the token from the TaskList
            foreach (dynamic token in commentTokens)
            {
                TaskIdentifiers.Add(token.Text);
            }

            taskListRegex = new Regex($"#\\s*({TaskIdentifiers.Aggregate((x, y) => x + "|" + y)})(\\W+.*)");
        }

        private static void StartTaskListThread()
        {
            var thread = new System.Threading.Thread(() =>
            {
                while(true)
                {
                    try
                    {
                        System.Threading.Thread.Sleep(5000);
                        LoadTaskListTokens();
                    }
                    catch
                    {

                    }
                }
            });
            thread.Start();
        }

        private void CreateProvider()
        {
            if (PowerShellToolsPackage.Instance == null) return;

            if (taskProvider == null)
            {
                taskProvider = new TodoWindowTaskProvider(PowerShellToolsPackage.Instance);
                taskProvider.ProviderName = "To Do";
                LoadTaskListTokens();
                StartTaskListThread();
            }

            if (errorListProvider == null)
            {
                errorListProvider = new ErrorListProvider(PowerShellToolsPackage.Instance);
                PowerShellToolsPackage.Instance.AnalysisService.OnAnalysisResults += AnalysisServiceCallback_OnAnalysisResults;
            }
        }

        private void AnalysisServiceCallback_OnAnalysisResults(object send, AnalysisResult e)
        {
            var tasksToRemove = new List<ErrorTask>();
            foreach (var task in errorListProvider.Tasks)
            {
                var errorTask = task as ErrorTask;
                if (errorTask != null && errorTask.Document.Equals(e.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    tasksToRemove.Add(errorTask);
                }
            }

            tasksToRemove.ForEach(x => errorListProvider.Tasks.Remove(x));

            foreach (var error in e.Issues)
            {
                var errorTask = new ErrorTask();
                errorTask.Text = error.Message;
                errorTask.ErrorCategory = error.Severity == "3" ? TaskErrorCategory.Error : TaskErrorCategory.Warning;
                errorTask.Line = error.Line - 1;
                errorTask.Column = error.Column;
                errorTask.Document = e.FileName;
                errorTask.Priority = TaskPriority.High;

                errorListProvider.Tasks.Add(errorTask);
                errorTask.Navigate += (sender, args) =>
                {
                    try
                    {
                        var dte = (PowerShellToolsPackage.Instance.GetService(typeof(DTE))) as DTE;
                        if (dte != null)
                        {
                            var document = dte.Documents.Open(e.FileName);
                            document.Activate();
                            var ts = dte.ActiveDocument.Selection as TextSelection;
                            ts.GotoLine(error.Line, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceCommon.Log("Failed to navigate. " + ex.Message);
                    }
                };
            }
        }

        public PowerShellTokenizationService(ITextBuffer textBuffer, AnalysisWatcher watcher)
        {
            _textBuffer = textBuffer;
            _classifierService = new ClassifierService();
            _regionAndBraceMatchingService = new RegionAndBraceMatchingService();
            _textSnapshots = new BlockingCollection<ITextSnapshot>();
            this.watcher = watcher;

            AnalysisOptions.Instance.ScriptAnalyzerChanged += Instance_ScriptAnalyzerChanged;
            AnalysisOptions.Instance.SolutionWideAnalysisChanged += Instance_SolutionWideAnalysisChanged;
            AnalysisOptions.Instance.AnalyzeOnSaveChanged += Instance_AnalyzeOnSaveChanged;

            scriptAnalyzerEnabled = AnalysisOptions.Instance.ScriptAnalyzer;
            analyzeOnSave = AnalysisOptions.Instance.AnalyzeOnSave;

            var solutionWide = AnalysisOptions.Instance.SolutionWideAnalysis;
            if (solutionWide)
            {
                this.watcher.Enabled = true;
                this.watcher.ForceAnalysis();
            }

            _textBuffer.OnSave(AnalyzeFile);

            _ = TokenizationTaskAsync();
            StartTokenization();
        }

        private void Instance_AnalyzeOnSaveChanged(object sender, DebugEngine.EventArgs<bool> e)
        {
            analyzeOnSave = e.Value;
        }

        private void AnalyzeFile()
        {
            if (analyzeOnSave)
            {
                var filePath = _textBuffer.GetFilePath();
                var currentSnapshot = _textBuffer.CurrentSnapshot;
                var requestId = PowerShellToolsPackage.Instance.AnalysisService.RequestStringAnalysis(filePath, currentSnapshot.GetText());
                AnalysisRequests.AddOrUpdate(requestId, currentSnapshot, (x, y) => currentSnapshot);
            }
        }

        private void Instance_SolutionWideAnalysisChanged(object sender, DebugEngine.EventArgs<bool> e)
        {
            this.watcher.Enabled = e.Value;

            if (this.watcher.Enabled)
            {
                this.watcher.ForceAnalysis();
            }
        }

        private void Instance_ScriptAnalyzerChanged(object sender, DebugEngine.EventArgs<bool> e)
        {
            scriptAnalyzerEnabled = e.Value;
        }

        public void StartTokenization()
        {
            _textSnapshots.Add(_textBuffer.CurrentSnapshot);
        }

        private Tasks.Task TokenizationTaskAsync()
        {
            return Tasks.Task.Run(async () =>
            {
                while(true)
                {
                    var snapshot = _textSnapshots.Take();

                    if (_lastSnapshot == null || _textBuffer.CurrentSnapshot.Version == snapshot.Version)
                    {
                        try
                        {
                            var currentSnapshot = snapshot;
                            string scriptToTokenize = currentSnapshot.GetText();

                            var filePath = snapshot.TextBuffer.GetFilePath();

                            Ast genereatedAst;
                            Token[] generatedTokens;
                            List<ClassificationInfo> tokenSpans;
                            List<TagInformation<ErrorTag>> errorTags = null;
                            Dictionary<int, int> startBraces;
                            Dictionary<int, int> endBraces;
                            List<TagInformation<IOutliningRegionTag>> regions;
                            Tokenize(scriptToTokenize, 0, out genereatedAst, out generatedTokens, out tokenSpans, out startBraces, out endBraces, out regions);

                            if (scriptAnalyzerEnabled && PowerShellToolsPackage.Instance?.AnalysisService != null && !string.IsNullOrEmpty(filePath) && !analyzeOnSave)
                            {
                                var requestId = PowerShellToolsPackage.Instance.AnalysisService.RequestStringAnalysis(filePath, currentSnapshot.GetText());
                                AnalysisRequests.AddOrUpdate(requestId, currentSnapshot, (x, y) => currentSnapshot);
                            }

                            if (_textBuffer.CurrentSnapshot.Version.VersionNumber == currentSnapshot.Version.VersionNumber)
                            {
                                try
                                {
                                    if (errorTags != null)
                                    {
                                        UpdateErrorList(errorTags, currentSnapshot);
                                    }

                                    await UpdateTaskListAsync(generatedTokens, currentSnapshot);
                                }
                                catch (Exception ex)
                                {
                                    Log.Warn("Failed to update task list!", ex);
                                }

                                SetTokenizationProperties(genereatedAst, generatedTokens, tokenSpans, errorTags, startBraces, endBraces, regions);
                                RemoveCachedTokenizationProperties();
                                _lastSnapshot = currentSnapshot;

                                OnTokenizationComplete(genereatedAst);
                                NotifyOnTagsChanged(BufferProperties.Classifier, currentSnapshot);

                                NotifyOnTagsChanged(typeof(PowerShellOutliningTagger).Name, currentSnapshot);
                                NotifyBufferUpdated();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Failed to tokenize the new snapshot.", ex);
                        }
                    }
                }
            });
        }

        private void UpdateErrorList(IEnumerable<TagInformation<ErrorTag>> errorTags, ITextSnapshot currentSnapshot)
        {
            CreateProvider();

            if (errorListProvider == null) return;

            var filePath = currentSnapshot.TextBuffer.GetFilePath();

            var tasksToRemove = new List<ErrorTask>();
            foreach (var task in errorListProvider.Tasks)
            {
                var errorTask = task as ErrorTask;
                if (errorTask != null && errorTask.Document.Equals(filePath, StringComparison.OrdinalIgnoreCase) && errorTask.ErrorCategory == TaskErrorCategory.Error)
                {
                    tasksToRemove.Add(errorTask);
                }
            }

            tasksToRemove.ForEach(x => errorListProvider.Tasks.Remove(x));

            foreach (var error in errorTags)
            {
                var errorTask = new ErrorTask();
                errorTask.Text = error.Tag.ToolTipContent.ToString();
                errorTask.ErrorCategory = TaskErrorCategory.Error;
                errorTask.Line = error.Start;
                errorTask.Document = filePath;
                errorTask.Priority = TaskPriority.High;

                errorListProvider.Tasks.Add(errorTask);
            }
        }

        private async Tasks.Task UpdateTaskListAsync(IEnumerable<Token> generatedTokens, ITextSnapshot currentSnapshot)
        {
            CreateProvider();

            if (taskProvider == null) return;

            taskProvider.Tasks.Clear();
            foreach (var token in generatedTokens.Where(m => m.Kind == TokenKind.Comment && taskListRegex.IsMatch(m.Text)))
            {
                var errorTask = new ErrorTask();
                errorTask.CanDelete = false;
                errorTask.Category = TaskCategory.Comments;
                errorTask.Document = currentSnapshot.TextBuffer.GetFilePath();
                errorTask.Line = token.Extent.StartLineNumber - 1;
                errorTask.Column = token.Extent.StartColumnNumber;
                errorTask.Navigate += async (sender, args) =>
                {
                    var dte = (await PowerShellToolsPackage.Instance.GetServiceAsync(typeof (DTE))) as DTE;
                    if (dte != null)
                    {
                        var document = dte.Documents.Open(currentSnapshot.TextBuffer.GetFilePath());
                        document.Activate();
                        var ts = dte.ActiveDocument.Selection as TextSelection;
                        ts.GotoLine(token.Extent.StartLineNumber, true);
                    }
                };

                errorTask.Text = token.Text.Substring(1);
                errorTask.Priority = TaskPriority.Normal;
                errorTask.IsPriorityEditable = true;

                taskProvider.Tasks.Add(errorTask);
            }

            var taskList = await PowerShellToolsPackage.Instance.GetServiceAsync(typeof (SVsTaskList)) as IVsTaskList2;
            if (taskList == null)
            {
                return;
            }

            var guidProvider = typeof (TodoWindowTaskProvider).GUID;
            taskList.SetActiveProvider(ref guidProvider);
        }

        private void NotifyOnTagsChanged(string name, ITextSnapshot currentSnapshot)
        {
            INotifyTagsChanged classifier;
            if (_textBuffer.Properties.TryGetProperty<INotifyTagsChanged>(name, out classifier))
            {
                classifier.OnTagsChanged(new SnapshotSpan(currentSnapshot, new Span(0, currentSnapshot.Length)));
            }
        }

        private void NotifyBufferUpdated()
        {
            INotifyBufferUpdated tagger;
            if (_textBuffer.Properties.TryGetProperty<INotifyBufferUpdated>(typeof(PowerShellBraceMatchingTagger).Name, out tagger) && tagger != null)
            {
                tagger.OnBufferUpdated(_textBuffer);
            }
        }

        private void SetBufferProperty(object key, object propertyValue)
        {
            if (_textBuffer.Properties.ContainsProperty(key))
            {
                _textBuffer.Properties.RemoveProperty(key);
            }
            _textBuffer.Properties.AddProperty(key, propertyValue);
        }

        private void OnTokenizationComplete(Ast generatedAst)
        {
            if (TokenizationComplete != null)
            {
                TokenizationComplete(this, generatedAst);
            }
        }

        private void Tokenize(
                      string spanText,
                      int startPosition,
                      out Ast generatedAst,
                      out Token[] generatedTokens,
                      out List<ClassificationInfo> tokenSpans,
                      out Dictionary<int, int> startBraces,
                      out Dictionary<int, int> endBraces,
                      out List<TagInformation<IOutliningRegionTag>> regions)
        {
            Log.Debug("Parsing input.");

            generatedAst =   Parser.ParseInput(spanText, out generatedTokens, out ParseError[] errs);

            Log.Debug("Classifying tokens.");
            tokenSpans = _classifierService.ClassifyTokens(generatedTokens, startPosition).ToList();

            Log.Debug("Matching braces and regions.");
            _regionAndBraceMatchingService.GetRegionsAndBraceMatchingInformation(spanText, startPosition, generatedTokens, out startBraces, out endBraces, out regions);
        }

        private void SetTokenizationProperties(Ast generatedAst,
                              Token[] generatedTokens,
                              List<ClassificationInfo> tokenSpans,
                              List<TagInformation<ErrorTag>> errorTags,
                              Dictionary<int, int> startBraces,
                              Dictionary<int, int> endBraces,
                              List<TagInformation<IOutliningRegionTag>> regions)
        {
            SetBufferProperty(BufferProperties.Ast, generatedAst);
            SetBufferProperty(BufferProperties.Tokens, generatedTokens);
            SetBufferProperty(BufferProperties.TokenSpans, tokenSpans);
            SetBufferProperty(BufferProperties.TokenErrorTags, errorTags);
            SetBufferProperty(BufferProperties.StartBraces, startBraces);
            SetBufferProperty(BufferProperties.EndBraces, endBraces);
            SetBufferProperty(BufferProperties.Regions, regions);
        }

        private void RemoveCachedTokenizationProperties()
        {
            if (_textBuffer.Properties.ContainsProperty(BufferProperties.RegionTags))
            {
                _textBuffer.Properties.RemoveProperty(BufferProperties.RegionTags);
            }
        }
    }

    internal struct BraceInformation
    {
        internal char Character;
        internal int Position;

        internal BraceInformation(char character, int position)
        {
            Character = character;
            Position = position;
        }
    }

    internal struct ClassificationInfo
    {
        private readonly IClassificationType _classificationType;
        private readonly int _length;
        private readonly int _start;

        internal ClassificationInfo(int start, int length, IClassificationType classificationType)
        {
            _classificationType = classificationType;
            _start = start;
            _length = length;
        }

        internal int Length
        {
            get { return _length; }
        }

        internal int Start
        {
            get { return _start; }
        }

        internal IClassificationType ClassificationType
        {
            get { return _classificationType; }
        }
    }

    internal struct TagInformation<T> where T : ITag
    {
        internal readonly int Length;
        internal readonly int Start;
        internal readonly T Tag;

        internal TagInformation(int start, int length, T tag)
        {
            Tag = tag;
            Start = start;
            Length = length;
        }

        internal TagSpan<T> GetTagSpan(ITextSnapshot snapshot)
        {
            return snapshot.Length >= Start + Length ?
            new TagSpan<T>(new SnapshotSpan(snapshot, Start, Length), Tag) : null;
        }
    }

    public static class BufferProperties
    {
        public const string Ast = "PSAst";
        public const string Tokens = "PSTokens";
        public const string TokenErrorTags = "PSTokenErrorTags";
        public const string AnalysisErrorTags = "PSTokenAnalysisTags";
        public const string EndBraces = "PSEndBrace";
        public const string StartBraces = "PSStartBrace";
        public const string TokenSpans = "PSTokenSpans";
        public const string Regions = "PSRegions";
        public const string RegionTags = "PSRegionTags";
        public const string Classifier = "Classifier";
        public const string ErrorTagger = "PowerShellErrorTagger";
        public const string AnalysisTagger = "ScriptAnalysisTagger";
        public const string FromRepl = "PowerShellREPL";
        public const string LastWordReplacementSpan = "LastWordReplacementSpan";
        public const string LineUpToReplacementSpan = "LineUpToReplacementSpan";
        public const string SessionOriginIntellisense = "SessionOrigin_Intellisense";
        public const string SessionCompletionFullyMatchedStatus = "SessionCompletionFullyMatchedStatus";
        public const string PowerShellTokenizer = "PowerShellTokenizer";
        public const string ScriptAnalyzerQuickFixer = "ScriptAnalyzerQuickFixer";
    }

    public interface INotifyTagsChanged
    {
        void OnTagsChanged(SnapshotSpan span);
    }

    public interface INotifyBufferUpdated
    {
        void OnBufferUpdated(ITextBuffer textBuffer);
    }
}


