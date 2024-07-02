using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Moq;
using PowerShellTools.LanguageService;

namespace PowerShellTools.Test.LanguageService
{
    /// <summary>
    /// Tests finding function definitions in a script
    /// </summary>
    [TestClass]
    public class NavigationUnitTests
    {
        /// <summary>
        /// Return the function definition
        /// </summary>
        [TestMethod]
        public void FindFunctionDefinition()
        {
            var script = new List<ScriptSectionMock>()
            {
                /*
                function zero { 0 }
                zero
                */
                new ScriptSectionMock("function ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 1) }),
                new ScriptSectionMock(" { 0 }\n", null ),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 1) })
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return nothing if the function...
        /// 1. ...is referenced before it is defined
        /// 2. ...is referenced outside of the scope it is defined
        /// 3. ...is not defined
        /// </summary>
        [TestMethod]
        public void FunctionDefinitionNotFound()
        {
            var script = new List<ScriptSectionMock>()
            {
                /*
                { 
                    zero
                    function zero { 0 }
                }
                zero
                bogus
                */
                new ScriptSectionMock("{\n\tzero\n\tfunction ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 2) }),
                new ScriptSectionMock(" { 0 }\n}\nzero\nbogus", null),
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return the first definition before the reference in the same scope
        /// Else return the definition outside the scope
        /// </summary>
        [TestMethod]
        public void FindFunctionDefinitionByScope()
        {
            var script = new List<ScriptSectionMock>()
            {
                /*
                { 
                    zero
                    function zero { 0 }
                    zero
                }
                function zero { 0 }
                */
                new ScriptSectionMock("{\n\t", null ),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){ new ExpectedDefinitionMock("zero", 6) }),
                new ScriptSectionMock("\n\tfunction ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 3) }),
                new ScriptSectionMock(" { 0 }\n\t", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 3) }),
                new ScriptSectionMock("\n}\nfunction ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 6) }),
                new ScriptSectionMock(" { 0 }\n", null),
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return all possible definitions when there is no definition in the scope of the reference and multiple outside the scope
        /// </summary>
        [TestMethod]
        public void FindAmbiguousFunctionDefinition()
        {
            var script = new List<ScriptSectionMock>()
            {
                /*
                { 
                    zero
                }
                function zero { 0 }
                function zero { 0 }
                */
                new ScriptSectionMock("{\n\t", null ),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>()
                {
                    new ExpectedDefinitionMock("zero", 4),
                    new ExpectedDefinitionMock("zero", 5)
                }),
                new ScriptSectionMock("}\nfunction ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 4) }),
                new ScriptSectionMock(" { 0 }\n", null),
                new ScriptSectionMock("\nfunction ", null),
                new ScriptSectionMock("zero", new List<ExpectedDefinitionMock>(){  new ExpectedDefinitionMock("zero", 5) }),
                new ScriptSectionMock(" { 0 }\n", null),
            };

            ValidateDefinitions(script);
        }

        [TestMethod]
        public void GetFunctionNameSpan()
        {
            ValidateFunctionNameSpan("filter filt {}", new Span(7, 4));
            ValidateFunctionNameSpan("function func ($funcVar) {}", new Span(9, 4));
            ValidateFunctionNameSpan("class Animal { Animal() {} }", new Span(15, 6));
        }

        private void ValidateFunctionNameSpan(string mockedScript, Span expectedResult)
        {
            var generatedAst = GenerateAst(mockedScript);
            var functionDefinition = generatedAst.Find(ast => ast is FunctionDefinitionAst, true) as FunctionDefinitionAst;
            var functionNameSpan = NavigationExtensions.GetFunctionNameSpan(functionDefinition);

            if (functionDefinition == null)
            {
                // If functionDefinition is null, that means the test is running with a version of PowerShell that doesn't support classes, so span should be null
                Assert.IsNull(functionNameSpan);
            }
            else
            {
                Assert.AreEqual(expectedResult, functionNameSpan);
            }
        }

        private struct ScriptSectionMock
        {
            public string Code;
            public IEnumerable<ExpectedDefinitionMock> ExpectedValues;

            public ScriptSectionMock(string code, IEnumerable<ExpectedDefinitionMock> expectedValues)
            {
                Code = code;
                ExpectedValues = expectedValues;
            }
        }

        private struct ExpectedDefinitionMock
        {
            public string Name;
            public int StartLineNumber;

            public ExpectedDefinitionMock(string name, int startLineNumber)
            {
                Name = name;
                StartLineNumber = startLineNumber;
            }
        }

        private static void ValidateDefinitions(IEnumerable<ScriptSectionMock> script)
        {
            var mockedScript = String.Concat(script.Select(s => s.Code));
            var textSnapshotMock = TextSnapshotMock(mockedScript);

            var includeEnd = 0;
            var previousCodeLength = 0;
            foreach (var scriptSection in script)
            {
                int start = previousCodeLength + includeEnd;
                includeEnd = scriptSection.ExpectedValues != null ? 1 : 0;

                for (var i = start; i < previousCodeLength + scriptSection.Code.Length + includeEnd; i++)
                {
                    var generatedAst = GenerateAst(mockedScript);
                    var actualVals = NavigationExtensions.FindFunctionDefinitions(generatedAst, textSnapshotMock, i);

                    if (scriptSection.ExpectedValues == null)
                    {
                        Assert.IsNull(actualVals);
                    }
                    else
                    {
                        Assert.IsNotNull(actualVals);
                        Assert.AreEqual(scriptSection.ExpectedValues.Count(), actualVals.Count());
                        scriptSection.ExpectedValues.Zip(actualVals, (expected, actual) =>
                        {
                            Assert.AreEqual(expected.Name, actual.Name);
                            Assert.AreEqual(expected.StartLineNumber, actual.Extent.StartLineNumber);
                            return true;
                        });
                    }
                }

                previousCodeLength = previousCodeLength + scriptSection.Code.Length;
            }
        }

        private static ITextSnapshot TextSnapshotMock(string mockedScript)
        {
            var textSnapshotMock = new Mock<ITextSnapshot>();
            textSnapshotMock.Setup(t => t.Length).Returns(mockedScript.Length);
            return textSnapshotMock.Object;
        }

        private static Ast GenerateAst(string script)
        {
            Token[] generatedTokens;
            ParseError[] errors;
            return Parser.ParseInput(script, out generatedTokens, out errors);
        }
    }
}