using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Moq;
using PowerShellTools.TestAdapter;
using PowerShellTools.TestAdapter.Helpers;

namespace PowerShellTools.Test.TestAdapter
{
    [TestClass]
    [Ignore]
    public class PowerShellTestContainerDiscovererTest
    {
		[TestMethod]
	    public void ShouldDiscoverTestFilesWithHyphens()
		{
			var solutionProvider = new Mock<ISolutionProvider>();
			var solution = new Mock<ISolution>();
			var project = new Mock<IProject>();
			var logger = new Mock<ILogger>();
			var solutionEventListener = new Mock<ISolutionEventsListener>();
			var testFilesUpdateWatcher = new Mock<ITestFilesUpdateWatcher>();
			var testFilesAddRemoveListener = new Mock<ITestFileAddRemoveListener>();

			solutionProvider.Setup(m => m.GetLoadedSolution()).Returns(solution.Object);
			solution.Setup(m => m.Projects).Returns(new[] {project.Object});
			project.Setup(m => m.Items).Returns(new[] {"My-Test.tests.ps1"});
			project.Setup(m => m.IsPowerShellProject).Returns(true);

		    var discoverer = new PowerShellTestContainerDiscoverer(solutionProvider.Object, logger.Object, solutionEventListener.Object, testFilesUpdateWatcher.Object, testFilesAddRemoveListener.Object);

			testFilesAddRemoveListener.Raise(m => m.TestFileChanged += null, this, new TestFileChangedEventArgs("My-Test.tests.ps1", TestFileChangedReason.Added));

			Assert.IsTrue(discoverer.TestContainers.Any(m => m.Source == "My-Test.tests.ps1"));
		}
    }
}
