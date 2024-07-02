using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace PowerShellTools.TestAdapter
{
    [DefaultExecutorUri(PowerShellTestExecutor.ExecutorUriString)]
    [FileExtension(".ps1")]
    public class PowerShellTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext,
            IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            GetTests(sources, discoverySink, logger);
        }

        public static List<TestCase> GetTests(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink, IMessageLogger logger = null)
        {
            var tests = new List<TestCase>();
            foreach (var source in sources)
            {
                DiscoverPesterTests(discoverySink, logger, source, tests);    
            }
            return tests;
        }

        private static void DiscoverPesterTests(ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string source,
            List<TestCase> tests)
        {
            SendMessage(TestMessageLevel.Informational, string.Format("Searching for tests in {0}", source), logger);
            Token[] tokens;
            ParseError[] errors;
            var ast = Parser.ParseFile(source, out tokens, out errors);

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    SendMessage(TestMessageLevel.Error, string.Format("Error parsing file. ", error.Message), logger);
                }
                return;
            }

            var testSuites =
                ast.FindAll(
                    m =>
                        (m is CommandAst) &&
                        string.Equals("describe", (m as CommandAst).GetCommandName(), StringComparison.OrdinalIgnoreCase), true);

            foreach (var ast1 in testSuites)    
            {
                var describeName = GetFunctionName(logger, ast1, "describe");

                var tags = GetDescribeTags(logger, ast1);

                var its = ast1.FindAll(
                m =>
                    (m is CommandAst) &&
                    (m as CommandAst).GetCommandName() != null &&
                    (m as CommandAst).GetCommandName().Equals("it", StringComparison.OrdinalIgnoreCase), true);

                foreach (var test in its)
                {
                    var itAst = (CommandAst)test;
                    var itName = GetFunctionName(logger, test, "it");
                    var contextName = GetParentContextName(logger, test);

                    // Didn't find the name for the test. Skip it.
                    if (String.IsNullOrEmpty(itName))
                    {
                        SendMessage(TestMessageLevel.Informational, "Test name was empty. Skipping test.", logger);
                        continue;
                    }

                    var fullName = String.Format("{0}.{1}.{2}", describeName, contextName, itName);

	                var testcase = new TestCase(fullName, PowerShellTestExecutor.ExecutorUri, source)
	                {
		                DisplayName = itName,
		                CodeFilePath = source,
		                LineNumber = itAst.Extent.StartLineNumber,
					};

	                foreach (var tag in tags)
	                {
		                testcase.Traits.Add(tag, string.Empty);
	                }

	                SendMessage(TestMessageLevel.Informational,
		                string.Format("Adding test {0} from {1} at {2}", describeName, source, testcase.LineNumber), logger);

	                if (discoverySink != null)
	                {
		                discoverySink.SendTestCase(testcase);
	                }

	                tests.Add(testcase);
				}
            }
        }

        private static string GetParentContextName(IMessageLogger logger, Ast ast)
        {
            if  (ast.Parent is CommandAst &&
                string.Equals("context", (ast.Parent as CommandAst).GetCommandName(), StringComparison.OrdinalIgnoreCase))
            {
                return GetFunctionName(logger, ast.Parent, "context");
            }
            
            if (ast.Parent != null)
            {
                return GetParentContextName(logger, ast.Parent);
            }

            return "No Context";
        }

		private static string GetFunctionName(IMessageLogger logger, Ast context, string functionName)
        {
            var contextAst = (CommandAst) context;
            var contextName = string.Empty;
            bool nextElementIsName1 = false;
            foreach (var element in contextAst.CommandElements)
            {
                if (element is StringConstantExpressionAst &&
                    !(element as StringConstantExpressionAst).Value.Equals(functionName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    contextName = (element as StringConstantExpressionAst).Value;
                    break;
                }

                if (nextElementIsName1 && element is StringConstantExpressionAst)
                {
                    contextName = (element as StringConstantExpressionAst).Value;
                    break;
                }

                if (element is CommandParameterAst &&
                    (element as CommandParameterAst).ParameterName.Equals("Name",
                        StringComparison.OrdinalIgnoreCase))
                {
                    nextElementIsName1 = true;
                }
            }

            return contextName;
        }

        private static IEnumerable<string> GetDescribeTags(IMessageLogger logger, Ast context)
        {
            var contextAst = (CommandAst)context;
            var contextName = string.Empty;
            bool nextElementIsName1 = false;
            foreach (var element in contextAst.CommandElements)
            {
                if (nextElementIsName1)
                {
                    var tagStrings = element.FindAll(m => m is StringConstantExpressionAst, true);
                    foreach(StringConstantExpressionAst tag in tagStrings)
                    {
                        yield return tag.Value;
                    }
                    break;
                }

                if (element is CommandParameterAst &&
                    "tags".Contains((element as CommandParameterAst).ParameterName.ToLower()))
                {
                    nextElementIsName1 = true;
                }
            }
        }

        private static void SendMessage(TestMessageLevel level, string message, IMessageLogger logger)
        {
            if (logger != null)
            {
                logger.SendMessage(level, message);
            }
        }
    }
}