using System;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Moq;
using PowerShellTools.Classification;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Test.IntelliSense
{
    [TestClass]
    public class UtilitiesUnitTests
    {
        #region Test IsInCommentArea
        [TestMethod]
        public void TestEmptyScript()
        {
            IsInCommentAreaTestHelper("", 0, false);
        }

        [TestMethod]
        public void TestNoCommentScript()
        {
            string script = @"param(
                                [parameter(Mandatory=$true)]
                                [string]$someStr
                                )";
            IsInCommentAreaTestHelper(script, 0, false);
            IsInCommentAreaTestHelper(script, 10, false);
            IsInCommentAreaTestHelper(script, script.Length, false);
        }

        [TestMethod]
        public void TestAllLineCommentScript()
        {
            string script = @"#param(
                              #  [parameter(Mandatory=$true)]
                              #  [string]$someStr
                              #  )";
            IsInCommentAreaTestHelper(script, 0, false);
            IsInCommentAreaTestHelper(script, 1, true);
            IsInCommentAreaTestHelper(script, 50, true);
            IsInCommentAreaTestHelper(script, script.Length - 1, true);
            IsInCommentAreaTestHelper(script, script.Length, true);
        }

        [TestMethod]
        public void TestAllBlockCommentScript()
        {
            string script = @"<# param(
                                 [parameter(Mandatory=$true)]
                                 [string]$someStr
                                 )
                              #>";
            IsInCommentAreaTestHelper(script, 0, false);
            IsInCommentAreaTestHelper(script, 20, true);
            IsInCommentAreaTestHelper(script, script.Length - 1, true);
            IsInCommentAreaTestHelper(script, script.Length, true);
        }

        [TestMethod]
        public void TestNormalCommentScript()
        {
            string script = @"<# This is a block comment
                              #>
                              param(
                                [parameter(Mandatory=$true)]
                                [string]$someStr
                                )
                              # This is line comment 1
                              # This is line comment 2";
            IsInCommentAreaTestHelper(script, 0, false);
            IsInCommentAreaTestHelper(script, 10, true);
            IsInCommentAreaTestHelper(script, 100, false);
            IsInCommentAreaTestHelper(script, 300, true);
            IsInCommentAreaTestHelper(script, script.Length - 1, true);
            IsInCommentAreaTestHelper(script, script.Length, true);
        }

        private void IsInCommentAreaTestHelper(string script, int caretPosition, bool expected)
        {
            UtilitiesTestHelper(script, caretPosition, expected, Utilities.IsInCommentArea);
        }

        #endregion

        #region Test IsInStringArea

        [TestMethod]
        public void TestScriptWithoutString()
        {
            string script = @"param(
                                [parameter(Mandatory=$true)]
                                [string]$someStr
                                )
                              $myVar = 2";
            IsInStringAreaTestHelper(script, -1, false);
            IsInStringAreaTestHelper(script, 0, false);
            IsInStringAreaTestHelper(script, 10, false);
            IsInStringAreaTestHelper(script, script.Length - 1, false);
            IsInStringAreaTestHelper(script, script.Length, false);
        }

        [TestMethod]
        public void TestEmptyString()
        {
            string script = "\"\"";
            IsInStringAreaTestHelper(script, -1, false);
            IsInStringAreaTestHelper(script, 0, false);
            IsInStringAreaTestHelper(script, 1, true);
            IsInStringAreaTestHelper(script, 2, false);
        }

        [TestMethod]
        public void TestNormalString()
        {
            string script = "\"Hello World!\"";
            IsInStringAreaTestHelper(script, -1, false);
            IsInStringAreaTestHelper(script, 0, false);
            IsInStringAreaTestHelper(script, 1, true);
            IsInStringAreaTestHelper(script, 6, true);
            IsInStringAreaTestHelper(script, script.Length - 1, true);
            IsInStringAreaTestHelper(script, script.Length, false);
            IsInStringAreaTestHelper(script, script.Length + 10, false);
        }


        private void IsInStringAreaTestHelper(string script, int caretPosition, bool expected)
        {
            UtilitiesTestHelper(script, caretPosition, expected, Utilities.IsInStringArea);
        }

        #endregion

        #region Test IsInParameterArea

        [TestMethod]
        public void TestScriptWithoutParameter()
        {
            string script = @"$stringVar = ""Hello World!""
                              Write-Host $stringVar";
            IsInParameterAreaTestHelper(script, -1, false);
            IsInParameterAreaTestHelper(script, 0, false);
            IsInParameterAreaTestHelper(script, 5, false);
            IsInParameterAreaTestHelper(script, script.Length - 1, false);
            IsInParameterAreaTestHelper(script, script.Length, false);
        }

        [TestMethod]
        public void TestScriptWithParameter()
        {
            string script = @"Set-ExecutionPolicy -ExecutionPolicy ";
            IsInParameterAreaTestHelper(script, -1, false);
            IsInParameterAreaTestHelper(script, 0, false);
            IsInParameterAreaTestHelper(script, 10, false);
            IsInParameterAreaTestHelper(script, 20, true);
            IsInParameterAreaTestHelper(script, 25, true);
            IsInParameterAreaTestHelper(script, script.Length - 2, true);
            IsInParameterAreaTestHelper(script, script.Length - 1, false);
            IsInParameterAreaTestHelper(script, script.Length, false);
        }

        private void IsInParameterAreaTestHelper(string script, int caretPosition, bool expected)
        {
            UtilitiesTestHelper(script, caretPosition, expected, Utilities.IsInParameterArea);
        }

        #endregion

        #region Test IsInVariableArea

        [TestMethod]
        public void TestScriptWithoutVariable()
        {
            string script = @"Write-Host ""Hello World!""";
            IsInVariableAreaTestHelper(script, -1, false);
            IsInVariableAreaTestHelper(script, 0, false);
            IsInVariableAreaTestHelper(script, 5, false);
            IsInVariableAreaTestHelper(script, script.Length - 1, false);
            IsInVariableAreaTestHelper(script, script.Length, false);
        }

        [TestMethod]
        public void TestScriptWithDefinedVariable()
        {
            string script = @"$stringVar = ""Hello World""";
            IsInVariableAreaTestHelper(script, -1, false);
            IsInVariableAreaTestHelper(script, 0, true);
            IsInVariableAreaTestHelper(script, 9, true);
            IsInVariableAreaTestHelper(script, 10, false);
            IsInVariableAreaTestHelper(script, script.Length - 1, false);
            IsInVariableAreaTestHelper(script, script.Length, false);
        }

        [TestMethod]
        public void TestScriptWithBuiltInVariable()
        {
            string script = @"$dte.ActiveWindow";
            IsInVariableAreaTestHelper(script, -1, false);
            IsInVariableAreaTestHelper(script, 0, true);
            IsInVariableAreaTestHelper(script, script.Length - 1, true);
            IsInVariableAreaTestHelper(script, script.Length, false);
        }

        private void IsInVariableAreaTestHelper(string script, int caretPosition, bool expected)
        {
            UtilitiesTestHelper(script, caretPosition, expected, Utilities.IsInVariableArea);
        }

        #endregion

        private void UtilitiesTestHelper(string script, int caretPosition, bool expected, Func<int, ITextBuffer, bool> UtitiliesFunc)
        {
            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            Mock<ITextBuffer> textBuffer = new Mock<ITextBuffer>();
            TextBufferMockHelper(textBuffer, script, tokens);
            bool actual = UtitiliesFunc(caretPosition, textBuffer.Object);

            Assert.AreEqual(expected, actual);
        }

        private void TextBufferMockHelper(Mock<ITextBuffer> textBuffer, string script, Token[] tokens)
        {
            PropertyCollection pc = new PropertyCollection();
            pc.AddProperty(BufferProperties.Tokens, tokens);
            textBuffer.Setup(m => m.Properties).Returns(pc);
            textBuffer.Setup(m => m.CurrentSnapshot.Length)
                      .Returns(() => script.Length);
        }
    }
}
