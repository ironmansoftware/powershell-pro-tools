using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ExportModuleMemberTests
    {
        [Fact]
        public void ShouldExportModuleMember()
        {
            var textEditorState = new TextEditorState
            {
                Content = "function Definition {\r\n\r\n}",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 1
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
                Type = RefactorType.ExportModuleMember
            });

            Assert.Equal($"{Environment.NewLine}Export-ModuleMember -Function 'Definition'", result.First().Content);
            Assert.Equal(TextEditType.Insert, result.First().Type);

            Assert.Equal(5, result.First().Start.Line);
            Assert.Equal(0, result.First().Start.Character);
        }
    }
}
