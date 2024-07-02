using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.LanguageService;

namespace PowerShellTools.Test.BreakpointValidation
{
    [TestClass]
    public class BreakpointValidationUnitTests
    {
        [TestMethod]
        public void BreakpointOnEmptyCommentLineIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 1);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnCommentLineIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 2);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnEmptyLineIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 4);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnCommentBlockStartTagIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 5);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointWithinCommentBlockIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 6);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnCommentBlockEndTagIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 7);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnAssignmentIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 9);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnIfStatementIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 11);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnElseStatementIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 13);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnDoStatementIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 17);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnWhileStatementIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 19);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnFunctionStatementIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 21);

            Assert.IsTrue(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnAttributeIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 27);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnParamBlockIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 28);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnParameterIsInvalid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 30);

            Assert.IsFalse(actual.IsValid);
        }

        [TestMethod]
        public void BreakpointOnNamedBlockIsValid()
        {
            var ast = GenerateAst(testScript);
            var actual = BreakpointValidationHelper.GetBreakpointPosition(ast, 33);

            Assert.IsTrue(actual.IsValid);
        }

        private Ast GenerateAst(string script)
        {
            Token[] tokens;
            ParseError[] errors;
            return Parser.ParseInput(script, out tokens, out errors);
        }

        private static string testScript = @"
#
# Line comment
#

<#
    Blockcomment
#>

$var = 1

if($true) {

} else {

}

do {

}while($true) 

function Foo {

}

function Get-Bar {

    [CmdletBinding()]
    param
    (
        [Parameter()]$p
    )

    begin {
    }
    
    process {
    }

    end {
    }
}
";
    }
}
