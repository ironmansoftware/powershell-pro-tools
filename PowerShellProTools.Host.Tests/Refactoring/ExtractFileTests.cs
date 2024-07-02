using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ExtractFileTests
    {
        [Fact]
        public void ShouldExtractFile()
        {
            var textEditorState = new TextEditorState {
                Content = "Get-Process",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 10
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
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.FileName, Value = "File.ps1" }
                },
                Type = RefactorType.ExtractFile
            });

            Assert.Equal("Get-Process", result.First().Content);
            Assert.Equal(TextEditType.NewFile, result.First().Type);
            Assert.Equal("C:\\File.ps1", result.First().FileName);

            Assert.Equal("& \"$PSScriptRoot\\File.ps1\"", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(11, result.Last().End.Character);
        }

        [Fact]
        public void ShouldExtractFileWithComment()
        {
            var textEditorState = new TextEditorState
            {
                Content = "#Test\r\nGet-Process",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 1,
                    Character = 10
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
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.FileName, Value = "File.ps1" }
                },
                Type = RefactorType.ExtractFile
            });

            Assert.Equal("#Test\r\nGet-Process", result.First().Content);
            Assert.Equal(TextEditType.NewFile, result.First().Type);
            Assert.Equal("C:\\File.ps1", result.First().FileName);

            Assert.Equal("& \"$PSScriptRoot\\File.ps1\"", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(1, result.Last().End.Line);
            Assert.Equal(11, result.Last().End.Character);
        }

        [Fact]
        public void ShouldExtractFileWithSpace()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process ",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 11
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
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.FileName, Value = "File.ps1" }
                },
                Type = RefactorType.ExtractFile
            });

            Assert.Equal("Get-Process ", result.First().Content);
            Assert.Equal(TextEditType.NewFile, result.First().Type);
            Assert.Equal("C:\\File.ps1", result.First().FileName);

            Assert.Equal("& \"$PSScriptRoot\\File.ps1\"", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(12, result.Last().End.Character);
        }
    }
}
