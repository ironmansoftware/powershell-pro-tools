using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ConvertToSplatTests
    {
        [Fact]
        public void ShouldConvertBasicCommandToSplat()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad'",
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal("$Parameters = @{\r\n\tName = 'Notepad'\r\n}\r\nGet-Process @Parameters", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(27, result.Last().End.Character);
        }

        [Fact]
        public void ShouldConvertCommandWithCommandToSplat()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Something (Get-Something)",
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal("$Parameters = @{\r\n\tName = 'Notepad'\r\n\tSomething = (Get-Something)\r\n}\r\nGet-Process @Parameters", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(54, result.Last().End.Character);
        }

        [Fact]
        public void ShouldConvertCommandWithBareStringToSplat()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name Notepad",
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal("$Parameters = @{\r\n\tName = 'Notepad'\r\n}\r\nGet-Process @Parameters", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(25, result.Last().End.Character);
        }

        [Fact]
        public void ShouldSupportSwitchParameters()
        {
            var command = "Export-Excel -Path $sTargetFile -WorksheetName $lsWorkSheetNames[$iSheets] -AutoNameRange -AutoSize -TableName $sTableName -ExcelChartDefinition $oChartDefinition";
            var expectedOutput = "$Parameters = @{\r\n" +
            "\tPath = $sTargetFile\r\n" +
            "\tWorksheetName = $lsWorkSheetNames[$iSheets]\r\n" +
            "\tAutoNameRange = $true\r\n" +
            "\tAutoSize = $true\r\n" +
            "\tTableName = $sTableName\r\n" +
            "\tExcelChartDefinition = $oChartDefinition\r\n" +
            "}\r\n";

            var textEditorState = new TextEditorState
            {
                Content = command,
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal(expectedOutput, result.First().Content);
        }

        [Fact]
        public void ShouldLeaveUnknownArguments()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name Notepad Something",
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal("$Parameters = @{\r\n\tName = 'Notepad'\r\n}\r\nGet-Process @Parameters Something", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(35, result.Last().End.Character);
        }

        [Fact]
        public void ShouldConvertColonArguments()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name:Notepad -Confirm:$true",
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
                Type = RefactorType.ConvertToSplat
            });

            Assert.Equal("$Parameters = @{\r\n\tName = 'Notepad'\r\n\tConfirm = $true\r\n}\r\nGet-Process @Parameters", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(40, result.Last().End.Character);
        }
    }
}
