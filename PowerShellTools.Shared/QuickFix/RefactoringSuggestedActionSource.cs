using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using PowerShellProTools.Host.Refactoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PowerShellTools.Shared.QuickFix
{
    public class RefactoringSuggestedActionSource : ISuggestedActionsSource
    {
        private readonly RefactoringSuggestedActionSourceProvider m_factory;
        private readonly ITextBuffer m_textBuffer;
        private readonly ITextView m_textView;

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public RefactoringSuggestedActionSource(RefactoringSuggestedActionSourceProvider testSuggestedActionsSourceProvider, ITextView textView, ITextBuffer textBuffer)
        {
            m_factory = testSuggestedActionsSourceProvider;
            m_textBuffer = textBuffer;
            m_textView = textView;
        }

        public System.Threading.Tasks.Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
#pragma warning disable VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
#pragma warning restore VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
            {
                var fileName = GetFileName();
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                try
                {
                    return RefactorService.GetValidRefactors(new RefactorRequest
                    {
                        EditorState = GetState(),
                        Properties = new List<RefactoringProperty>
                    {
                        new RefactoringProperty
                        {
                            Type = RefactorProperty.FileName,
                            Value = GetFileName()
                        }
                    }
                    }).Any();
                }
                catch
                {
                    return false;
                }
            });
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            var editorState = GetState();

            try
            {
                var refactors = RefactorService.GetValidRefactors(new RefactorRequest
                {
                    EditorState = editorState,
                    Properties = new List<RefactoringProperty>
                {
                    new RefactoringProperty
                    {
                        Type = RefactorProperty.FileName,
                        Value = GetFileName()
                    }
                }
                });

                return new SuggestedActionSet[] { new SuggestedActionSet(refactors.Select(m => new RefactoringSuggestedAction(m, editorState, m_textBuffer, range.Span))) };
            }
            catch
            {
                return new SuggestedActionSet[0];
            }
        }

        private string GetFileName()
        {
            try
            {
                var dte = (DTE)Package.GetGlobalService(typeof(DTE));

                if (dte.ActiveDocument == null)
                {
                    return string.Empty;
                }

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
            catch
            {
                return string.Empty;
            }
        }

        private TextEditorState GetState()
        {
            try
            {
                var startLine = m_textView.Selection.Start.Position.GetContainingLine();
                var startChar = m_textView.Selection.Start.Position.Position - startLine.Start.Position;

                var endLine = m_textView.Selection.End.Position.GetContainingLine();
                var endChar = m_textView.Selection.End.Position.Position - endLine.Start.Position;

                var dte = (DTE)Package.GetGlobalService(typeof(DTE));

                return new TextEditorState
                {
                    FileName = dte.ActiveDocument?.Path,
                    Content = m_textBuffer.CurrentSnapshot.GetText(),
                    SelectionStart = new TextPosition
                    {
                        Line = startLine.LineNumber,
                        Character = startChar
                    },
                    SelectionEnd = new TextPosition
                    {
                        Line = endLine.LineNumber,
                        Character = endChar
                    }
                };
            }
            catch
            {
                return new TextEditorState();
            }
        }

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
