using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PowerShellProTools.Host.Tests.Refactoring
{
    public class ReorderTest
    {
        [Fact]
        public void ShouldMoveParameterToTheLeftWithSwitch()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Path Test -Switch",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 31
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty> {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.Name,
                        Value = "Left"
                    }
                },
                Type = RefactorType.Reorder
            });

            Assert.Equal("Get-Process -Path Test -Name 'Notepad' -Switch", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(46, result.Last().End.Character);
        }

        [Fact]
        public void ShouldMoveParameterToTheLeft()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Path Test",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 31
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty> {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.Name,
                        Value = "Left"
                    }
                },
                Type = RefactorType.Reorder
            });

            Assert.Equal("Get-Process -Path Test -Name 'Notepad'", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(38, result.Last().End.Character);
        }

        [Fact]
        public void ShouldMoveParameterToTheRight()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Path Test",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 14
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty> {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.Name,
                        Value = "Right"
                    }
                },
                Type = RefactorType.Reorder
            });

            Assert.Equal("Get-Process -Path Test -Name 'Notepad'", result.First().Content);
            Assert.Equal(TextEditType.Replace, result.First().Type);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(0, result.Last().End.Line);
            Assert.Equal(38, result.Last().End.Character);
        }

        [Fact]
        public void ShouldNotMoveParameterToTheRight()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Path Test",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 31
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty> {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.Name,
                        Value = "Right"
                    }
                },
                Type = RefactorType.Reorder
            });

            Assert.False(result.Any());
        }

        [Fact]
        public void ShouldNotMoveParameterToTheLeft()
        {
            var textEditorState = new TextEditorState
            {
                Content = "Get-Process -Name 'Notepad' -Path Test",
                FileName = "C:\\MyFile.ps1",
                SelectionEnd = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 14
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty> {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.Name,
                        Value = "Left"
                    }
                },
                Type = RefactorType.Reorder
            });

            Assert.False(result.Any());
        }
    }
}
