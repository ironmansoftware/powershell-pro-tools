using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ExtractFunctionTests
    {
        [Fact]
        public void ShouldExtractFunction()
        {
            var textEditorState = new TextEditorState
            {
                Content = "$Variable = 'test'\r\n$MyOtherVariable = 123\r\nStart-Process $Variable\r\n$MyVariable",
                FileName = "C:\\MyFile.ps1",
                SelectionStart = new TextPosition
                {
                    Line = 2,
                    Character = 0
                },
                SelectionEnd = new TextPosition
                {
                    Line = 2,
                    Character = 23
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.Name, Value = "Start-Function" }
                },
                Type = RefactorType.ExtractFunction
            });
            Assert.Equal("function Start-Function\r\n{\r\n\tparam($Variable)\r\nStart-Process $Variable\r\n}\r\n", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(2, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(2, result.Last().End.Line);
            Assert.Equal(23, result.Last().End.Character);
        }

        [Fact]
        public void ShouldExtractFunctionWithoutParameters()
        {
            var textEditorState = new TextEditorState
            {
                Content = "$Variable = 'test'\r\nStart-Process\r\n$MyVariable",
                FileName = "C:\\MyFile.ps1",
                SelectionStart = new TextPosition
                {
                    Line = 1,
                    Character = 0
                },
                SelectionEnd = new TextPosition
                {
                    Line = 1,
                    Character = 12
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.Name, Value = "Start-Function" }
                },
                Type = RefactorType.ExtractFunction
            });
            Assert.Equal("function Start-Function\r\n{\r\nStart-Process\r\n}\r\n", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(1, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(1, result.Last().End.Line);
            Assert.Equal(13, result.Last().End.Character);
        }
    }
}
