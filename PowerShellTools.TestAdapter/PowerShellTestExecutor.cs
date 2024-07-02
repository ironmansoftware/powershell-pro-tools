using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.ObjectModel;

namespace PowerShellTools.TestAdapter
{
    [ExtensionUri(ExecutorUriString)]
    public class PowerShellTestExecutor : ITestExecutor
    {
        public void RunTests(IEnumerable<string> sources, IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();
            IEnumerable<TestCase> tests = PowerShellTestDiscoverer.GetTests(sources, null);
            RunTests(tests, runContext, frameworkHandle);
        }

        private static void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private static void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            ExecutionPolicy currentPolicy = ExecutionPolicy.Undefined;

            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-ExecutionPolicy");

                foreach (var result in ps.Invoke())
                {
                    currentPolicy = ((ExecutionPolicy)result.BaseObject);
                    break;
                }

                if ((currentPolicy <= policy || currentPolicy == ExecutionPolicy.Bypass) && currentPolicy != ExecutionPolicy.Undefined) //Bypass is the absolute least restrictive, but as added in PS 2.0, and thus has a value of '4' instead of a value that corresponds to it's relative restrictiveness
                    return;

                ps.Commands.Clear();

                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope).AddParameter("Force");
                ps.Invoke();
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            _mCancelled = false;
            SetupExecutionPolicy();

	        var testSets = new List<TestCaseSet>();
	        foreach (var testCase in tests)
	        {
		        var describe = testCase.FullyQualifiedName.Split('.').First();
		        var codeFile = testCase.CodeFilePath;

		        var testSet = testSets.FirstOrDefault(m => m.Describe.Equals(describe, StringComparison.OrdinalIgnoreCase) &&
		                                                   m.File.Equals(codeFile, StringComparison.OrdinalIgnoreCase));

		        if (testSet == null)
		        {
			        testSet = new TestCaseSet(codeFile, describe);
					testSets.Add(testSet);
		        }

				testSet.TestCases.Add(testCase);
	        }

            foreach (var testSet in testSets)
            {
                if (_mCancelled) break;

                var testOutput = new StringBuilder();

                try
                {
                    var testAdapter = new TestAdapterHost();
                    testAdapter.HostUi.OutputString = s =>
                    {
						if (!string.IsNullOrEmpty(s))
							testOutput.Append(s);
                    };

                    var runpsace = RunspaceFactory.CreateRunspace(testAdapter);
                    runpsace.Open();

                    using (var ps = PowerShell.Create())
                    {
                        ps.Runspace = runpsace;
	                    RunTestSet(ps, testSet, runContext);

	                    foreach (var testResult in testSet.TestResults)
	                    {
							frameworkHandle.RecordResult(testResult);
						}
                    }
                }
                catch (Exception ex)
                {
	                foreach (var testCase in testSet.TestCases)
	                {
						var testResult = new TestResult(testCase);
		                testResult.Outcome = TestOutcome.Failed;
		                testResult.ErrorMessage = ex.Message;
		                testResult.ErrorStackTrace = ex.StackTrace;
						frameworkHandle.RecordResult(testResult);
	                }
                }

                if (testOutput.Length > 0)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, testOutput.ToString());
                }
            }
        }

        public void Cancel()
        {
            _mCancelled = true;
        }

        public const string ExecutorUriString = "executor://PowerShellTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        private bool _mCancelled;

	    public void RunTestSet(PowerShell powerShell, TestCaseSet testCaseSet, IRunContext runContext)
	    {
			SetupExecutionPolicy();
            powerShell.AddCommand("Import-Module").AddParameter("Name", @"Pester").AddParameter("PassThru");
            var moduleInfo = powerShell.Invoke<PSModuleInfo>().FirstOrDefault();
            powerShell.Commands.Clear();

            if (powerShell.HadErrors)
            {
                var errorRecord = powerShell.Streams.Error.FirstOrDefault();
                var errorMessage = errorRecord == null ? string.Empty : errorRecord.ToString();

                throw new Exception("Failed to load Pester module. " + errorMessage);
            }

            var fi = new FileInfo(testCaseSet.File);
            var pester5 = moduleInfo.Version > new Version(5, 0, 0);

            if (pester5)
            {
                powerShell.AddCommand("Invoke-Pester")
                    .AddParameter("Path", fi.Directory.FullName)
                    .AddParameter("FullNameFilter", testCaseSet.Describe)
                    .AddParameter("PassThru");
                
            }
            else
            {
                powerShell.AddCommand("Invoke-Pester")
                    .AddParameter("Path", fi.Directory.FullName)
                    .AddParameter("TestName", testCaseSet.Describe)
                    .AddParameter("PassThru");
            }


		    var pesterResults = powerShell.Invoke();
		    powerShell.Commands.Clear();

            if (pester5)
            {
                var resultObject = pesterResults.FirstOrDefault(o => o.Properties["Tests"] != null);
                var results = (resultObject.Properties["Tests"].Value as IEnumerable<object>).Select(m => new PSObject(m));

                testCaseSet.ProcessTestResults(results, pester5);
            }
            else
            {
                var resultObject = pesterResults.FirstOrDefault(o => o.Properties["TestResult"] != null);
                var results = resultObject.Properties["TestResult"].Value as Array;
                var items = new List<PSObject>();
                foreach(PSObject item in results)
                {
                    items.Add(item);
                }

                testCaseSet.ProcessTestResults(items, pester5);
            }
		}
    }

}
