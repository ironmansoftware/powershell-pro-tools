using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ExtractVariableTest
    {
        [Fact]
        public void ShouldExtractVariable()
        {
            var textEditorState = new TextEditorState
            {
                Content = "\"This is a string\"",
                FileName = "C:\\MyFile.ps1",
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 6
                },
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 10
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.Name, Value = "MyVariable" }
                },
                Type = RefactorType.ExtractVariable
            });
            Assert.Equal("$MyVariable = 'is a'\r\n\"This $($MyVariable) string\"", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(18, result.Last().End.Character);
        }
    }
}
