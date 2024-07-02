using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class IntroduceUsingNamespaceTests
    {
        [Fact]
        public void ShouldIntroduceNamespace()
        {
            var textEditorState = new TextEditorState
            {
                Content = "[System.Console]::WriteLine('yes')",
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
                Type = RefactorType.IntroduceUsingNamespace
            });


            Assert.Equal("using namespace System\r\n", result.Last().Content);
            Assert.Equal(TextEditType.Insert, result.Last().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);

            Assert.Equal("[Console]", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.First().Start.Line);
            Assert.Equal(0, result.First().Start.Character);
            Assert.Equal(0, result.First().End.Line);
            Assert.Equal(16, result.First().End.Character);
        }
    }
}
