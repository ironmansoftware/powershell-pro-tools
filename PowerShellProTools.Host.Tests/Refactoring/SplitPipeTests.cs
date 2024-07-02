using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class SplitPipeTests
    {
        [Fact]
        public void ShouldSplitPipe()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' | Stop-Process | Start-Process",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 2
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>(),
                Type = RefactorType.SplitPipe
            });

            Assert.Equal("$Result = Get-Process -Name 'Notepad'\r\n$Result = $Result | Stop-Process\r\n$Result | Start-Process\r\nRemove-Variable -Name 'Result'\r\n", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(58, result.Last().End.Character);
        }
    }
}
