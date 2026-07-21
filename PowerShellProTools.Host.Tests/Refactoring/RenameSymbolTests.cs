using PowerShellProTools.Common.Refactoring;
using PowerShellProTools.Host;
using PowerShellProTools.Host.Refactoring;
using System.Linq;
using System.Management.Automation.Language;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class RenameSymbolTests
    {
        [Fact]
        public void ShouldRenameFunctionDefinitionAndReferences()
        {
            var textEditorState = new TextEditorState
            {
                Content = "function Get-Thing {\r\n}\r\nGet-Thing\r\n",
                FileName = "C:\\MyFile.psm1",
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 10
                },
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 10
                }
            };

            var ast = Parser.ParseInput(textEditorState.Content, out Token[] tokens, out ParseError[] errors);
            var result = new RenameSymbol().Refactor("Get-ItemThing", textEditorState, ast, new Workspace("C:\\")).ToArray();

            Assert.Equal(2, result.Length);
            Assert.All(result, edit => Assert.Equal("Get-ItemThing", edit.Content));
            Assert.Equal(TextEditType.Replace, result[0].Type);
            Assert.Equal(0, result[0].Start.Line);
            Assert.Equal(9, result[0].Start.Character);
            Assert.Equal(0, result[0].End.Line);
            Assert.Equal(18, result[0].End.Character);
            Assert.Equal(2, result[1].Start.Line);
            Assert.Equal(0, result[1].Start.Character);
            Assert.Equal(2, result[1].End.Line);
            Assert.Equal(9, result[1].End.Character);
        }

        [Fact]
        public void ShouldRenameFunctionFromReference()
        {
            var textEditorState = new TextEditorState
            {
                Content = "function Get-Thing {\r\n}\r\nGet-Thing\r\n",
                FileName = "C:\\MyFile.psm1",
                SelectionStart = new TextPosition
                {
                    Line = 2,
                    Character = 1
                },
                SelectionEnd = new TextPosition
                {
                    Line = 2,
                    Character = 1
                }
            };

            var ast = Parser.ParseInput(textEditorState.Content, out Token[] tokens, out ParseError[] errors);
            var result = new RenameSymbol().Refactor("Get-ItemThing", textEditorState, ast, new Workspace("C:\\")).ToArray();

            Assert.Equal(2, result.Length);
            Assert.All(result, edit => Assert.Equal("Get-ItemThing", edit.Content));
            Assert.Equal(0, result[0].Start.Line);
            Assert.Equal(9, result[0].Start.Character);
            Assert.Equal(2, result[1].Start.Line);
            Assert.Equal(0, result[1].Start.Character);
        }

        [Fact]
        public void ShouldNotRenameCommandsWithoutFunctionDefinitions()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process\r\n",
                FileName = "C:\\MyFile.psm1",
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 1
                },
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 1
                }
            };

            var ast = Parser.ParseInput(textEditorState.Content, out Token[] tokens, out ParseError[] errors);
            var result = new RenameSymbol().Refactor("Get-SomethingElse", textEditorState, ast, new Workspace("C:\\")).ToArray();

            Assert.Empty(result);
        }
    }
}
