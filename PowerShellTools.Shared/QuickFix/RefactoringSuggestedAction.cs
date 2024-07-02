using EnvDTE;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.Shared.QuickFix
{
    public class RefactoringSuggestedAction : ISuggestedAction
    {
        private readonly RefactorInfo _refactorInfo;
        private readonly TextEditorState _textEditorState;
        private readonly ITextBuffer _textBuffer;
        private readonly Span _span;

        public RefactoringSuggestedAction(RefactorInfo refactorInfo, TextEditorState textEditorState, ITextBuffer textBuffer, Span span)
        {
            _refactorInfo = refactorInfo;
            _textEditorState = textEditorState;
            _textBuffer = textBuffer;
            _span = span;
        }

        public bool HasActionSets => false;

        public string DisplayText => _refactorInfo.Name;

        public ImageMoniker IconMoniker => default(ImageMoniker);

        public string IconAutomationText => null;

        public string InputGestureText => null;

        public bool HasPreview => false;

        public void Dispose()
        {

        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        private string GetFileName()
        {
            var dte = (DTE)Package.GetGlobalService(typeof(DTE));
            var fileName = string.Empty;
            for (int i = 1; i < 100; i++)
            {
                var found = false;
                foreach (ProjectItem projectItem in dte.ActiveDocument.ProjectItem.ContainingProject.ProjectItems)
                {
                    if (projectItem.Document?.Name?.Equals($"script{i}.ps1", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    fileName = $"script{i}.ps1";
                    break;
                }
            }

            return fileName;
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            var result = RefactorService.Refactor(new RefactorRequest
            {
                Type = _refactorInfo.Type,
                EditorState = _textEditorState,
                Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.FileName,
                        Value = GetFileName()
                    }
                }
            });

            if (!result.Any()) return;

            var span = _textBuffer.CurrentSnapshot.CreateTrackingSpan(_span, SpanTrackingMode.EdgeInclusive);

            var edit = span.TextBuffer.CreateEdit();

            try
            {

                foreach (var textEdit in result)
                {
                    switch (textEdit.Type)
                    {
                        case TextEditType.Insert:
                            edit.Insert(textEdit.Start.Index, textEdit.Content);
                            break;
                        case TextEditType.Replace:
                            edit.Replace(textEdit.Start.Index, textEdit.End.Index - textEdit.Start.Index, textEdit.Content);
                            break;
                        case TextEditType.NewFile:
                            var dte = (DTE)Package.GetGlobalService(typeof(DTE));
                            File.WriteAllText(textEdit.FileName, textEdit.Content);
                            var projectItem = dte.ActiveDocument.ProjectItem.ContainingProject.ProjectItems.AddFromFile(textEdit.FileName);
                            break;
                    }
                }
                edit.Apply();
            }
            catch
            {
                edit.Cancel();
            }
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
