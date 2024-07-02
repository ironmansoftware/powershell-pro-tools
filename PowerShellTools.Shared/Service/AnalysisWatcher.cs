using Common.Analysis;
using PowerShellTools.Common.Logging;
using PowerShellTools.LanguageService;
using PowerShellTools.TestAdapter.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.Service
{
    [Export]
    public class AnalysisWatcher
    {
        private readonly ISolutionProvider _solutionProvider;
        private bool _initialContainerSearch;
        private ISolutionEventsListener _solutionListener;
        private ITestFileAddRemoveListener _testFilesAddRemoveListener;
        private ITestFilesUpdateWatcher _testFilesUpdateWatcher;
        private List<string> _cachedContainers;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AnalysisWatcher));

        public bool Enabled { get; set; }

        public static AnalysisWatcher Instance { get; set; }

        [ImportingConstructor]
        public AnalysisWatcher(
          ISolutionProvider solutionProvider,
          ISolutionEventsListener solutionListener,
          ITestFilesUpdateWatcher testFilesUpdateWatcher,
          ITestFileAddRemoveListener testFilesAddRemoveListener)
        {
            _cachedContainers = new List<string>();
            _initialContainerSearch = true;
            _solutionProvider = solutionProvider;

            _solutionListener = solutionListener;
            _testFilesUpdateWatcher = testFilesUpdateWatcher;
            _testFilesAddRemoveListener = testFilesAddRemoveListener;

            _testFilesAddRemoveListener.TestFileChanged += OnProjectItemChanged;
            _testFilesAddRemoveListener.StartListeningForTestFileChanges();

            _solutionListener.SolutionUnloaded += SolutionListenerOnSolutionUnloaded;
            _solutionListener.SolutionProjectChanged += OnSolutionProjectChanged;
            _solutionListener.StartListeningForChanges();

            _testFilesUpdateWatcher.FileChangedEvent += OnProjectItemChanged;

            var testFiles = FindPowerShellTestFiles();
            UpdateFileWatcher(testFiles, true);

            Instance = this;
        }

        public void ForceAnalysis()
        {
            foreach(var item in _cachedContainers)
            {
                PowerShellToolsPackage.Instance?.AnalysisService?.RequestFileAnalysis(item);
            }
        }

        private void OnTestContainersChanged()
        {

        }

        private void SolutionListenerOnSolutionUnloaded(object sender, EventArgs eventArgs)
        {
            _initialContainerSearch = true;
        }

        private void OnSolutionProjectChanged(object sender, SolutionEventsListenerEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    IEnumerable<string> files = FindPowerShellTestFiles(e.Project);
                    if (e.ChangedReason == SolutionChangedReason.Load)
                    {
                        UpdateFileWatcher(files, true);
                    }
                    else if (e.ChangedReason == SolutionChangedReason.Unload)
                    {
                        UpdateFileWatcher(files, false);
                    }
                }
            }
            catch { }


            // Do not fire OnTestContainersChanged here.
            // This will cause us to fire this event too early before the UTE is ready to process containers and will result in an exception.
            // The UTE will query all the TestContainerDiscoverers once the solution is loaded.
        }

        private void UpdateFileWatcher(IEnumerable<string> files, bool isAdd)
        {
            foreach (string file in files)
            {
                if (isAdd)
                {
                    if (Enabled)
                    {
                        PowerShellToolsPackage.Instance.AnalysisService.RequestFileAnalysis(file);
                    }

                    _testFilesUpdateWatcher.AddWatch(file);
                    AddTestContainerIfTestFile(file);
                }
                else
                {
                    _testFilesUpdateWatcher.RemoveWatch(file);
                    RemoveTestContainer(file);
                }
            }
        }

        private void OnProjectItemChanged(object sender, TestFileChangedEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    // Don't do anything for files we are sure can't be test files
                    if (!IsPowerShellTestFile(e.File)) return;

                    switch (e.ChangedReason)
                    {
                        case TestFileChangedReason.Added:
                            if (Enabled)
                            {
                                PowerShellToolsPackage.Instance.AnalysisService.RequestFileAnalysis(e.File);
                            }

                            _testFilesUpdateWatcher.AddWatch(e.File);
                            AddTestContainerIfTestFile(e.File);

                            break;
                        case TestFileChangedReason.Removed:
                            _testFilesUpdateWatcher.RemoveWatch(e.File);
                            RemoveTestContainer(e.File);

                            break;
                        case TestFileChangedReason.Changed:
                            if (Enabled)
                            {
                                PowerShellToolsPackage.Instance.AnalysisService.RequestFileAnalysis(e.File);
                            }

                            AddTestContainerIfTestFile(e.File);
                            break;
                    }

                    OnTestContainersChanged();
                }
            }
            catch { }
        }

        private void AddTestContainerIfTestFile(string file)
        {
            bool isTestFile = IsPowerShellTestFile(file);
            RemoveTestContainer(file); // Remove if there is an existing container

            // If this is a test file
            if (isTestFile)
            {
                _cachedContainers.Add(file);
            }
        }

        private void RemoveTestContainer(string file)
        {
            int index = _cachedContainers.FindIndex(x => x.Equals(file, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _cachedContainers.RemoveAt(index);
            }
        }

        private IEnumerable<string> FindPowerShellTestFiles()
        {
            var solution = _solutionProvider.GetLoadedSolution();

            return solution.Projects.Where(m => m.IsPowerShellProject).SelectMany(FindPowerShellTestFiles).ToList();
        }

        private IEnumerable<string> FindPowerShellTestFiles(IProject project)
        {
            if (!project.IsPowerShellProject)
                return new string[0];

            return from item in project.Items
                   where IsPowerShellTestFile(item)
                   select item;
        }

        private bool IsPowerShellTestFile(string path)
        {
            try
            {
                return path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase);
            }
            catch (IOException)
            {
            }

            return false;
        }


        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_testFilesUpdateWatcher != null)
                {
                    _testFilesUpdateWatcher.FileChangedEvent -= OnProjectItemChanged;
                    ((IDisposable)_testFilesUpdateWatcher).Dispose();
                    _testFilesUpdateWatcher = null;
                }

                if (_testFilesAddRemoveListener != null)
                {
                    _testFilesAddRemoveListener.TestFileChanged -= OnProjectItemChanged;
                    _testFilesAddRemoveListener.StopListeningForTestFileChanges();
                    _testFilesAddRemoveListener = null;
                }

                if (_solutionListener != null)
                {
                    _solutionListener.SolutionProjectChanged -= OnSolutionProjectChanged;
                    _solutionListener.StopListeningForChanges();
                    _solutionListener = null;
                }
            }
        }
    }
}
