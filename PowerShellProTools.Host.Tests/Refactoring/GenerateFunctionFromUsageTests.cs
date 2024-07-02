using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class GenerateFunctionFromUsageTests
    {
        [Fact]
        public void ShouldGenerateAFunctionBasedOnParameters()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Start-Something -Name 'Notepad' -Test 'Cool'",
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
                Type = RefactorType.GenerateFunctionFromUsage
            });

            Assert.Equal("function Start-Something\r\n{\r\n\tparam(\r\n\t\t[Parameter()]\r\n\t\t$Name,\r\n\t\t[Parameter()]\r\n\t\t$Test\r\n\t)\r\n}\r\n", result.First().Content);
            Assert.Equal(TextEditType.Insert, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
        }

        [Fact]
        public void ShouldGenerateAFunctionBasedOnParametersAndArgs()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Start-Something -Name 'Notepad' 'Cool'",
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
                Type = RefactorType.GenerateFunctionFromUsage
            });

            Assert.Equal("function Start-Something\r\n{\r\n\tparam(\r\n\t\t[Parameter()]\r\n\t\t$Name,\r\n\t\t[Parameter(Position = 1)]\r\n\t\t$Parameter1\r\n\t)\r\n}\r\n", result.First().Content);
            Assert.Equal(TextEditType.Insert, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
        }

        [Fact]
        public void ShouldGenerateAdvancedFunctionIfPiped()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Something | Start-Something -Name 'Notepad' 'Cool'",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 18
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>(),
                Type = RefactorType.GenerateFunctionFromUsage
            });

            Assert.Equal("function Start-Something\r\n{\r\n\t[CmdletBinding()]\r\n\tparam(\r\n\t\t[Parameter(ValueFromPipeline = $true)]\r\n\t\t$InputObject,\r\n\t\t[Parameter()]\r\n\t\t$Name,\r\n\t\t[Parameter(Position = 1)]\r\n\t\t$Parameter1\r\n\t)\r\n\tProcess\r\n\t{\r\n\t}\r\n}\r\n", result.First().Content);
            Assert.Equal(TextEditType.Insert, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
        }
    }
}
