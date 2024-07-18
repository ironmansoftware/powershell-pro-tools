using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;

namespace PowerShellProTools.Host.Refactoring
{
    public class ExtractFile : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo { Name = "Extract selection to file", Type = RefactorType.ExtractFile };

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return state.SelectionStart.Line < state.SelectionEnd.Line || state.SelectionEnd.Character > state.SelectionStart.Character;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var startExtent = ast.Find(m =>
                m.Extent.StartLineNumber == state.SelectionStart.Line + 1 &&
                m.Extent.StartColumnNumber >= state.SelectionStart.Character + 1, true)?.Extent;

            var endExtent = ast.Find(m =>
                m.Extent.EndLineNumber == state.SelectionEnd.Line + 1 &&
                m.Extent.EndColumnNumber >= state.SelectionEnd.Character + 1, true)?.Extent;

            if (startExtent == null || endExtent == null || properties == null)
            {
                yield break;
            }

            var fileContent = state.Content.Substring(startExtent.StartOffset, endExtent.EndOffset - startExtent.StartOffset);

            var fileName = properties.FirstOrDefault(m => m.Type == RefactorProperty.FileName)?.Value;

            if (fileName == null) yield break;

            var fileInfo = new FileInfo(state.FileName);
            var fullPath = Path.Combine(fileInfo.DirectoryName, fileName);

            yield return new TextEdit { Content = fileContent, Type = TextEditType.NewFile, FileName = fullPath };

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = $"& \"$PSScriptRoot\\{fileName}\"",
                Start = new TextPosition
                {
                    Line = startExtent.StartLineNumber - 1,
                    Character = startExtent.StartColumnNumber - 1
                },
                End = new TextPosition
                {
                    Line = endExtent.EndLineNumber - 1,
                    Character = endExtent.EndColumnNumber - 1
                },
                FileName = state.FileName,
                Type = TextEditType.Replace
            };


        }
    }
}
