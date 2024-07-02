using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ConvertToMultiLineCommandTests
    {
        [Fact]
        public void ShouldConvertBasicCommand()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Force -Cool 'Nice'",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 0
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>(),
                Type = RefactorType.ConvertToMultiline
            });

            Assert.Equal("Get-Process -Name 'Notepad'`\r\n-Force`\r\n-Cool 'Nice'", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(textEditorState.Content.Length, result.Last().End.Character);
        }
    }
}
