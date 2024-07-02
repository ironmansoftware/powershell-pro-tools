using System;
using System.Management.Automation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.TestAdapter;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;

namespace PowerShellTools.Test.TestAdapter
{
    [TestClass]
    [Ignore]
    public class PowerShellTestExecutorTest
    {
        private PowerShellTestExecutor _executor;
        private string _tempFile;
        private string _pesterTestDir;
        private Mock<IRunContext> _runContext;
        private Runspace _runspace;
        private PowerShell _powerShell;

        [TestInitialize]
        public void Init()
        {
            _executor = new PowerShellTestExecutor();

            _runContext = new Mock<IRunContext>();

            _runspace = RunspaceFactory.CreateRunspace(new TestAdapterHost());
            _runspace.Open();
            _powerShell = PowerShell.Create();
            _powerShell.Runspace = _runspace;
        }

        [TestCleanup]
        public void Clean()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }

            if (Directory.Exists(_pesterTestDir))
            {
                Directory.Delete(_pesterTestDir);
            }

            if (_runspace != null)
            {
                _runspace.Dispose();
            }
        }

	    private TestCaseSet WriteTestFile(string name, string contents)
	    {
		    _pesterTestDir = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());

		    Directory.CreateDirectory(_pesterTestDir);

		    _tempFile = Path.Combine(_pesterTestDir, "MyTests.Tests.ps1");
		    File.WriteAllText(_tempFile, contents);

		    var testCase = new TestCase(name, new Uri("http://test.com"), _tempFile);
		    testCase.CodeFilePath = _tempFile;

		    var describe = name.Split('.').First();
		    var set = new TestCaseSet(_tempFile, describe);
		    set.TestCases.Add(testCase);
		    return set;
	    }

        [TestMethod]
        public void ShouldReturnSuccessfulTestResults()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                     It 'Should pass' {
                         1 | Should be 1
                     }
                }
            }
            ";

            var testCase = WriteTestFile("Test.Blah.Should pass", testScript);
	        _executor.RunTestSet(_powerShell, testCase, _runContext.Object);
	        var result = testCase.TestResults.First();

			Assert.AreEqual(TestOutcome.Passed, result.Outcome);
        }

		[TestMethod]
        public void ShouldRunTestWithoutContext()
        {
            const string testScript = @"
            Describe 'Test' {
                It 'Should pass' {
                    1 | Should be 1
                }
            }
            ";

            var testCase = WriteTestFile("Test.No Context.Should pass", testScript);
	        _executor.RunTestSet(_powerShell, testCase, _runContext.Object);
	        var result = testCase.TestResults.First();

			Assert.AreEqual(TestOutcome.Passed, result.Outcome);
        }

        [TestMethod]
        public void ShouldRunTestWithoutContextFailure()
        {
            const string testScript = @"
            Describe 'Test' {
                It 'Should pass' {
                    1 | Should be 2
                }
            }
            ";

            var testCase = WriteTestFile("Test.No Context.Should pass", testScript);
	        _executor.RunTestSet(_powerShell, testCase, _runContext.Object);
	        var result = testCase.TestResults.First();

			Assert.AreEqual(TestOutcome.Failed, result.Outcome);
        }


        [TestMethod]
        public void ShouldReturnUnsuccessfulTestResult()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        1 | Should Be 2
                    }
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Failed, result.Outcome);
        }

        [TestMethod]
        public void ShouldFindSkippedTests()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        1 | Should Be 2
                    } -Skip
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Skipped, result.Outcome);
        }


        [TestMethod]
        public void ShouldFindPendingTestsWithNoAsserts()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                    } 
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Skipped, result.Outcome);
        }

        [TestMethod]
        public void ShouldFindPendingTests()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                    } -Pending
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Skipped, result.Outcome);
        }

        [TestMethod]
        public void ShouldReturnUnsuccessfulTestResultForAnException()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        throw 'This sucks!'
                    }
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Failed, result.Outcome);
        }

        [TestMethod]
        public void UnsuccessfulItBlockShouldOverrideSuccessfulItBlock()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        1 | Should be 2
                    }

                    It 'Should succeed' {
                        1 | Should be 1
                    }
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Failed, result.Outcome);
        }

        [TestMethod]
        public void SkippedItBlockShouldOverrideSuccessfulItBlock()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' -Skip {
                        1 | Should be 1
                    }

                    It 'Should succeed' {
                        1 | Should be 1
                    }
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Skipped, result.Outcome);
        }

        [TestMethod]
        public void PendingItBlockShouldOverrideSuccessfulItBlock()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' -Pending {
                        1 | Should be 1
                    }

                    It 'Should succeed' {
                        1 | Should be 1
                    }
                }
            }
            ";

            var testFile = WriteTestFile("Test.Blah.Should fail", testScript);

	        _executor.RunTestSet(_powerShell, testFile, _runContext.Object);
	        var result = testFile.TestResults.First();

			Assert.AreEqual(TestOutcome.Skipped, result.Outcome);
        }
    }
}
