using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
            if (TryRunTestSetOutOfProcess(testCaseSet, runContext, out var outOfProcessError))
            {
                return;
            }

            if (!string.IsNullOrEmpty(outOfProcessError))
            {
                throw new Exception(outOfProcessError);
            }

            try
            {
                RunTestSetInProcess(powerShell, testCaseSet);
            }
            catch
            {
                if (string.IsNullOrEmpty(outOfProcessError) && TryRunTestSetOutOfProcess(testCaseSet, runContext, out outOfProcessError, true))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(outOfProcessError))
                {
                    throw new Exception(outOfProcessError);
                }

                throw;
            }
		}

        private void RunTestSetInProcess(PowerShell powerShell, TestCaseSet testCaseSet)
        {
            SetupExecutionPolicy();
            powerShell.AddCommand("Import-Module").AddParameter("Name", @"Pester").AddParameter("PassThru");
            var moduleInfo = powerShell.Invoke<PSModuleInfo>().FirstOrDefault();
            powerShell.Commands.Clear();

            if (powerShell.HadErrors || moduleInfo == null)
            {
                var errorRecord = powerShell.Streams.Error.FirstOrDefault();
                var errorMessage = errorRecord == null ? string.Empty : errorRecord.ToString();

                throw new Exception("Failed to load Pester module. " + errorMessage);
            }

            var fi = new FileInfo(testCaseSet.File);
            var pester5 = moduleInfo.Version >= new Version(5, 0, 0);

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

        private bool TryRunTestSetOutOfProcess(TestCaseSet testCaseSet, IRunContext runContext, out string errorMessage, bool fallbackToPowerShellCore = false)
        {
            errorMessage = null;

            var powerShellExe = fallbackToPowerShellCore ? FindOnPath("pwsh.exe") : GetConfiguredPowerShellCore(testCaseSet, runContext);
            if (string.IsNullOrEmpty(powerShellExe))
            {
                return false;
            }

            var scriptPath = Path.Combine(Path.GetTempPath(), "PowerShellTools.Pester." + Guid.NewGuid().ToString("N") + ".ps1");
            try
            {
                File.WriteAllText(scriptPath, GetOutOfProcessRunnerScript(), Encoding.UTF8);

                var startInfo = new ProcessStartInfo
                {
                    FileName = powerShellExe,
                    Arguments = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File " + QuoteArgument(scriptPath) +
                                " -TestPath " + QuoteArgument(new FileInfo(testCaseSet.File).Directory.FullName) +
                                " -Describe " + QuoteArgument(testCaseSet.Describe),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    var output = outputTask.Result;
                    var error = errorTask.Result;
                    var results = ParseOutOfProcessResults(output);

                    if (process.ExitCode != 0 && !results.Any())
                    {
                        errorMessage = "Failed to run Pester with PowerShell Core. " + (string.IsNullOrWhiteSpace(error) ? output : error);
                        return false;
                    }

                    testCaseSet.ProcessTestResults(results);
                    return true;
                }
            }
            finally
            {
                if (File.Exists(scriptPath))
                {
                    File.Delete(scriptPath);
                }
            }
        }

        private static string GetConfiguredPowerShellCore(TestCaseSet testCaseSet, IRunContext runContext)
        {
            var projectFile = FindProjectFile(testCaseSet.File, runContext?.SolutionDirectory);
            if (projectFile == null)
            {
                return null;
            }

            var powerShellVersion = GetPowerShellVersion(projectFile);
            if (string.IsNullOrEmpty(powerShellVersion) ||
                powerShellVersion.Equals("Windows PowerShell", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!TryParsePowerShellCoreVersion(powerShellVersion, out var version))
            {
                return null;
            }

            return FindPowerShellCore(version) ?? FindOnPath("pwsh.exe");
        }

        private static bool TryParsePowerShellCoreVersion(string powerShellVersion, out Version version)
        {
            if (!Version.TryParse(powerShellVersion, out version))
            {
                var match = Regex.Match(powerShellVersion, @"\d+(\.\d+)*");
                if (!match.Success || !Version.TryParse(match.Value, out version))
                {
                    return false;
                }
            }

            return version.Major >= 6;
        }

        private static string FindProjectFile(string testFile, string solutionDirectory)
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(testFile));
            while (directory != null)
            {
                var projectFile = directory.GetFiles("*.pssproj").FirstOrDefault();
                if (projectFile != null)
                {
                    return projectFile.FullName;
                }

                if (!string.IsNullOrEmpty(solutionDirectory) &&
                    directory.FullName.Equals(solutionDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string GetPowerShellVersion(string projectFile)
        {
            var document = XDocument.Load(projectFile);
            var ns = document.Root.Name.Namespace;
            return document.Descendants(ns + "PowerShellVersion").Select(m => m.Value).FirstOrDefault();
        }

        private static string FindPowerShellCore(Version version)
        {
            var programFiles = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var programFilesPath in programFiles.Where(m => !string.IsNullOrEmpty(m)))
            {
                var pwshPath = Path.Combine(programFilesPath, "PowerShell", version.Major.ToString(), "pwsh.exe");
                if (File.Exists(pwshPath))
                {
                    return pwshPath;
                }
            }

            return null;
        }

        private static string FindOnPath(string fileName)
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            foreach (var directory in path.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    continue;
                }

                var candidate = Path.Combine(directory.Trim(), fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static IEnumerable<PesterTestResult> ParseOutOfProcessResults(string output)
        {
            var results = new List<PesterTestResult>();
            using (var reader = new StringReader(output ?? string.Empty))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("__PSTEST__", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var parts = line.Substring("__PSTEST__".Length).Split('\t');
                    if (parts.Length != 4)
                    {
                        continue;
                    }

                    results.Add(new PesterTestResult
                    {
                        FullName = DecodeField(parts[0]),
                        Result = parts[1],
                        ErrorMessage = DecodeField(parts[2]),
                        ErrorStackTrace = DecodeField(parts[3])
                    });
                }
            }

            return results;
        }

        private static string DecodeField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        private static string GetOutOfProcessRunnerScript()
        {
            return @"
param(
    [string]$TestPath,
    [string]$Describe
)

$ErrorActionPreference = 'Stop'

function ConvertTo-EncodedField {
    param($Value)

    if ($null -eq $Value) {
        $Value = ''
    }

    [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes([string]$Value))
}

function Write-PesterTestResult {
    param(
        [string]$FullName,
        [string]$Result,
        [string]$ErrorMessage,
        [string]$ErrorStackTrace
    )

    Write-Output (""__PSTEST__{0}`t{1}`t{2}`t{3}"" -f (ConvertTo-EncodedField $FullName), $Result, (ConvertTo-EncodedField $ErrorMessage), (ConvertTo-EncodedField $ErrorStackTrace))
}

Import-Module Pester -ErrorAction Stop
$module = Get-Module Pester | Sort-Object Version -Descending | Select-Object -First 1

if ($module.Version.Major -ge 5) {
    $run = Invoke-Pester -Path $TestPath -FullNameFilter $Describe -PassThru
    foreach ($test in $run.Tests) {
        $path = @($test.Path)
        if ($path.Count -eq 2) {
            $fullName = ""$($path[0]).No Context.$($path[1])""
        }
        elseif ($path.Count -gt 2) {
            $fullName = ""$($path[0]).$($path[$path.Count - 2]).$($path[$path.Count - 1])""
        }
        else {
            $fullName = $test.Name
        }

        $errorRecord = @($test.ErrorRecord | Where-Object { $null -ne $_ }) | Select-Object -First 1
        $errorMessage = if ($errorRecord -and $errorRecord.Exception) { $errorRecord.Exception.Message } else { '' }
        $errorStackTrace = if ($errorRecord) { $errorRecord.ScriptStackTrace } else { '' }

        Write-PesterTestResult -FullName $fullName -Result $test.Result -ErrorMessage $errorMessage -ErrorStackTrace $errorStackTrace
    }
}
else {
    $run = Invoke-Pester -Path $TestPath -TestName $Describe -PassThru
    foreach ($test in $run.TestResult) {
        $context = if ([string]::IsNullOrEmpty($test.Context)) { 'No Context' } else { $test.Context }
        $fullName = ""$($test.Describe).$context.$($test.Name)""
        Write-PesterTestResult -FullName $fullName -Result $test.Result -ErrorMessage $test.FailureMessage -ErrorStackTrace $test.StackTrace
    }
}
";
        }
    }

}
