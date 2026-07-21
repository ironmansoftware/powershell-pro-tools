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
            Assert.Equal("function Start-Function\r\n{\r\n    param(\r\n        $Variable\r\n    )\r\n\r\n    Start-Process $Variable\r\n}\r\n", result.Last().Content);
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
            Assert.Equal("function Start-Function\r\n{\r\n    Start-Process\r\n}\r\n", result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(1, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(1, result.Last().End.Line);
            Assert.Equal(13, result.Last().End.Character);
        }

        [Fact]
        public void ShouldExtractIndentedFunctionWithReadBeforeWriteParameters()
        {
            var content = "    if($ParamSet -eq 'Name') {\r\n" +
                          "        $Filter = \"`$_.DisplayName -like '$CurrentApp'\"\r\n" +
                          "    } elseif($ParamSet -eq 'GUID') {\r\n" +
                          "        # format GUID as brackets w/ hyphens\r\n" +
                          "        $CurrentApp = $CurrentApp.ToString('b')\r\n" +
                          "        $Filter = \"`$_.PSChildName -eq '$($CurrentApp.ToString('b'))'\"\r\n" +
                          "    }\r\n" +
                          "\r\n" +
                          "    if($Version) {\r\n" +
                          "        $Filter += \" -and [version]'$Version' -ge `$_.DisplayVersion\"\r\n" +
                          "    }\r\n" +
                          "\r\n" +
                          "    $Filter += \" -and `$_.UninstallString -ne `$null\"\r\n" +
                          "    $FilterScript = [scriptblock]::Create($Filter)\r\n" +
                          "\r\n" +
                          "    $Results = @(Get-ItemProperty -Path $RegKeys | Where-Object -FilterScript $FilterScript)\r\n" +
                          "\r\n" +
                          "    log \"Found $($Results.Count) applications matching $CurrentApp $Version\"";

            var textEditorState = new TextEditorState
            {
                Content = content,
                FileName = "C:\\MyFile.ps1",
                SelectionStart = new TextPosition
                {
                    Line = 0,
                    Character = 0
                },
                SelectionEnd = new TextPosition
                {
                    Line = 17,
                    Character = 72
                }
            };

            var result = RefactorService.Refactor(new RefactorRequest
            {
                EditorState = textEditorState,
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty { Type = RefactorProperty.Name, Value = "FuncName" }
                },
                Type = RefactorType.ExtractFunction
            });

            var expected = "    function FuncName\r\n" +
                           "    {\r\n" +
                           "        param(\r\n" +
                           "            [string]$ParamSet,\r\n" +
                           "            $CurrentApp,\r\n" +
                           "            [version]$Version,\r\n" +
                           "            [string]$RegKeys\r\n" +
                           "        )\r\n" +
                           "\r\n" +
                           "        if($ParamSet -eq 'Name') {\r\n" +
                           "            $Filter = \"`$_.DisplayName -like '$CurrentApp'\"\r\n" +
                           "        } elseif($ParamSet -eq 'GUID') {\r\n" +
                           "            # format GUID as brackets w/ hyphens\r\n" +
                           "            $CurrentApp = $CurrentApp.ToString('b')\r\n" +
                           "            $Filter = \"`$_.PSChildName -eq '$($CurrentApp.ToString('b'))'\"\r\n" +
                           "        }\r\n" +
                           "\r\n" +
                           "        if($Version) {\r\n" +
                           "            $Filter += \" -and [version]'$Version' -ge `$_.DisplayVersion\"\r\n" +
                           "        }\r\n" +
                           "\r\n" +
                           "        $Filter += \" -and `$_.UninstallString -ne `$null\"\r\n" +
                           "        $FilterScript = [scriptblock]::Create($Filter)\r\n" +
                           "\r\n" +
                           "        $Results = @(Get-ItemProperty -Path $RegKeys | Where-Object -FilterScript $FilterScript)\r\n" +
                           "\r\n" +
                           "        log \"Found $($Results.Count) applications matching $CurrentApp $Version\"\r\n" +
                           "    }\r\n";

            Assert.Equal(expected, result.Last().Content);
            Assert.Equal(TextEditType.Replace, result.Last().Type);
            Assert.Equal("C:\\MyFile.ps1", result.Last().FileName);
            Assert.Equal(0, result.Last().Start.Line);
            Assert.Equal(0, result.Last().Start.Character);
            Assert.Equal(17, result.Last().End.Line);
            Assert.Equal(76, result.Last().End.Character);
        }
    }
}
