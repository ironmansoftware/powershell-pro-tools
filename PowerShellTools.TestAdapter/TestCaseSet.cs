using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace PowerShellTools.TestAdapter
{
	public class TestCaseSet
	{
		private List<TestResult> _testResults;

		public TestCaseSet(string fileName, string describe)
		{
			File = fileName;
			Describe = describe;
			TestCases = new List<TestCase>();
		}

		public string File { get; }
		public string Describe { get; }
		public List<TestCase> TestCases { get; }
		public IEnumerable<TestResult> TestResults { get { return _testResults; } }

		public void ProcessTestResults(IEnumerable<PSObject> results, bool pester5)
		{
			_testResults = new List<TestResult>();

			foreach (PSObject result in results)
			{
				if (pester5)
                {
					ProcessPester5Results(result);
				}
				else
                {
					ProcessPesterResults(result);
                }
			}
		}

		private void ProcessPester5Results(PSObject result)
		{
			var path = result.Properties["Path"].Value as List<string>;

			var testFullName = string.Empty;
			if (path.Count == 2)
            {
				testFullName = $"{path[0]}.No Context.{path[1]}";
            }
			else if (path.Count == 3)
			{
				testFullName = $"{path[0]}.{path[1]}.{path[2]}";
			}

			// Skip test cases we aren't trying to run
			var testCase = TestCases.FirstOrDefault(m => m.FullyQualifiedName == testFullName);
			if (testCase == null) return;
			var testResult = new TestResult(testCase);

			testResult.Outcome = GetOutcome(result.Properties["Result"].Value as string);

			var errorRecords = result.Properties["ErrorRecord"].Value as List<object>;

			var errorRecord = errorRecords.OfType<ErrorRecord>().FirstOrDefault();

			testResult.ErrorStackTrace = errorRecord?.ScriptStackTrace;
			testResult.ErrorMessage = errorRecord?.Exception?.Message;

			_testResults.Add(testResult);
		}

		private void ProcessPesterResults(PSObject result)
        {
			var describe = result.Properties["Describe"].Value as string;
			var name = result.Properties["Name"].Value as string;
			if (!HandleParseError(result, describe, name))
			{
				return;
			}

			var context = result.Properties["Context"].Value as string;

			if (string.IsNullOrEmpty(context))
				context = "No Context";

			// Skip test cases we aren't trying to run
			var testCase = TestCases.FirstOrDefault(m => m.FullyQualifiedName == string.Format("{0}.{1}.{2}", describe, context, name));
			if (testCase == null) return;
			var testResult = new TestResult(testCase);

			testResult.Outcome = GetOutcome(result.Properties["Result"].Value as string);

			var stackTraceString = result.Properties["StackTrace"].Value as string;
			var errorString = result.Properties["FailureMessage"].Value as string;

			testResult.ErrorStackTrace = stackTraceString;
			testResult.ErrorMessage = errorString;

			_testResults.Add(testResult);
		}

		private bool HandleParseError(PSObject result, string describe, string name)
		{
			var errorMessage = string.Format("Error in {0}", File);
			if (describe.Contains(errorMessage) || name.StartsWith("Error occurred in"))
			{
				var stackTraceString = result.Properties["StackTrace"].Value as string;
				var errorString = result.Properties["FailureMessage"].Value as string;

				foreach (var tc in TestCases)
				{
					var testResult = new TestResult(tc);
					testResult.Outcome = TestOutcome.Failed;
					testResult.ErrorMessage = errorString;
					testResult.ErrorStackTrace = stackTraceString;
					_testResults.Add(testResult);
				}

				return false;
			}

			return true;
		}

		private static TestOutcome GetOutcome(string testResult)
		{
			if (string.IsNullOrEmpty(testResult))
			{
				return TestOutcome.NotFound;
			}

			if (testResult.Equals("passed", StringComparison.OrdinalIgnoreCase))
			{
				return TestOutcome.Passed;
			}
			if (testResult.Equals("skipped", StringComparison.OrdinalIgnoreCase))
			{
				return TestOutcome.Skipped;
			}
			if (testResult.Equals("pending", StringComparison.OrdinalIgnoreCase))
			{
				return TestOutcome.Skipped;
			}
			return TestOutcome.Failed;
		}
	}
}
