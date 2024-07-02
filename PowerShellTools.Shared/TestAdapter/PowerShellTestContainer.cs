using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Extensibility.Model;

namespace PowerShellTools.TestAdapter
{
    public class PowerShellTestContainer : ITestContainer
    {
        private readonly ITestContainerDiscoverer _discoverer;
        private readonly DateTime _timeStamp;

        public PowerShellTestContainer(ITestContainerDiscoverer discoverer, string source, Uri executorUri)
            : this(discoverer, source, executorUri, Enumerable.Empty<Guid>())
        {
        }

        public PowerShellTestContainer(ITestContainerDiscoverer discoverer, string source, Uri executorUri,
            IEnumerable<Guid> debugEngines)
        {
            Source = source;
            ExecutorUri = executorUri;
            DebugEngines = debugEngines;
            _discoverer = discoverer;
            TargetFramework = FrameworkVersion.Framework45;
            TargetPlatform = Architecture.AnyCPU;
            _timeStamp = GetTimeStamp();
        }

        private PowerShellTestContainer(PowerShellTestContainer copy)
            : this(copy._discoverer, copy.Source, copy.ExecutorUri)
        {
            _timeStamp = copy._timeStamp;
        }

        public Uri ExecutorUri { get; set; }
        public string Source { get; set; }
        public IEnumerable<Guid> DebugEngines { get; set; }
        public FrameworkVersion TargetFramework { get; set; }
        public Architecture TargetPlatform { get; set; }

        public IDeploymentData DeployAppContainer()
        {
            return null;
        }

        public bool IsAppContainerTestContainer
        {
            get { return false; }
        }

        public ITestContainerDiscoverer Discoverer
        {
            get { return _discoverer; }
        }

        public int CompareTo(ITestContainer other)
        {
            var testContainer = other as PowerShellTestContainer;
            if (testContainer == null)
            {
                return -1;
            }

            int result = String.Compare(Source, testContainer.Source, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
            {
                return result;
            }

            int ts = _timeStamp.CompareTo(testContainer._timeStamp);
            return ts;
        }

        public ITestContainer Snapshot()
        {
            return new PowerShellTestContainer(this);
        }

        private DateTime GetTimeStamp()
        {
            if (!String.IsNullOrEmpty(Source) && File.Exists(Source))
            {
                return File.GetLastWriteTime(Source);
            }
            return DateTime.MinValue;
        }

        public override string ToString()
        {
            return ExecutorUri + "/" + Source;
            //return this.ExecutorUri.ToString();
        }
    }
}