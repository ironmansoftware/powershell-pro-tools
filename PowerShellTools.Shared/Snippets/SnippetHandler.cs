using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using MSXML;

namespace PowerShellTools.Snippets
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("snippets")]
    [ContentType("PowerShell")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class SnippetHandlerProvider : IVsTextViewCreationListener
    {

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            textView.Properties.GetOrCreateSingletonProperty(() => new SnippetHandler(textViewAdapter, this, AdapterService, textView));
        }
    }

    internal class SnippetHandler : IOleCommandTarget, IVsExpansionClient
    {
        private readonly IVsTextLines _lines;
        private readonly IVsTextView _view;
        private readonly ITextView _textView;
        private readonly IVsEditorAdaptersFactoryService _adapterFactory;
        private IVsExpansionSession _session;
        private bool _sessionEnded, _selectEndSpan;
        private ITrackingPoint _selectionStart, _selectionEnd;

        public const string SurroundsWith = "SurroundsWith";
        public const string SurroundsWithStatement = "SurroundsWithStatement";
        public const string Expansion = "Expansion";

        IVsExpansionManager _mExManager;
        private readonly IOleCommandTarget _mNextCommandHandler;
        private readonly SnippetHandlerProvider _mProvider;

        private static string[] _allStandardSnippetTypes = { Expansion, SurroundsWith };
        private static string[] _surroundsWithSnippetTypes = { SurroundsWith, SurroundsWithStatement };

        [Import]
        internal IVsEditorAdaptersFactoryService AdapterFactory { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        internal SnippetHandler(IVsTextView textViewAdapter, SnippetHandlerProvider provider, IVsEditorAdaptersFactoryService adaptersFactory, ITextView textView)
        {
            _adapterFactory = adaptersFactory;
            _textView = textView;
            _view = textViewAdapter;
            _mProvider = provider;
            //get the text manager from the service provider
            var textManager = (IVsTextManager2)_mProvider.ServiceProvider.GetService(typeof(SVsTextManager));
            textManager.GetExpansionManager(out _mExManager);
            _session = null;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out _mNextCommandHandler);

            _lines = (IVsTextLines)_adapterFactory.GetBufferAdapter(_textView.TextBuffer);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_mProvider.ServiceProvider))
            {
                return _mNextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            if (pguidCmdGroup != VSConstants.VSStd2K)
                return _mNextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            switch ((VSConstants.VSStd2KCmdID) nCmdID)
            {
                case VSConstants.VSStd2KCmdID.RETURN:
                    if (InSession && ErrorHandler.Succeeded(EndCurrentExpansion(false)))
                    {
                        return VSConstants.S_OK;
                    }
                    break;
                case VSConstants.VSStd2KCmdID.TAB:
                    if (InSession && ErrorHandler.Succeeded(NextField()))
                    {
                        return VSConstants.S_OK;
                    }
                    break;
                case VSConstants.VSStd2KCmdID.BACKTAB:
                    if (InSession && ErrorHandler.Succeeded(PreviousField()))
                    {
                        return VSConstants.S_OK;
                    }
                    break;
                case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                    var textManager = (IVsTextManager2)_mProvider.ServiceProvider.GetService(typeof(SVsTextManager));
                    textManager.GetExpansionManager(out _mExManager);
                    TriggerSnippet(nCmdID);
                    return VSConstants.S_OK;
            }

            return _mNextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void TriggerSnippet(uint nCmdID)
        {
            if (_mExManager == null) return;

            string prompt;
            string[] snippetTypes;
            if ((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.SURROUNDWITH)
            {
                prompt = "Surround with:";
                snippetTypes = _surroundsWithSnippetTypes;
            }
            else
            {
                prompt = "Insert snippet:";
                snippetTypes = _allStandardSnippetTypes;
            }

            _mExManager.InvokeInsertionUI(
                _view,
                this,
                new Guid(GuidList.PowerShellLanguage),
                snippetTypes, 
                snippetTypes.Length,
                0,
                null,
                0,
                0,
                prompt,
                ">"
                );
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (VsShellUtilities.IsInAutomationFunction(_mProvider.ServiceProvider))
                return _mNextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            if (pguidCmdGroup != VSConstants.VSStd2K || cCmds <= 0)
                return _mNextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            if (prgCmds[0].cmdID != (uint) VSConstants.VSStd2KCmdID.INSERTSNIPPET && (prgCmds[0].cmdID != (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH || _textView.Selection.IsEmpty))
                return _mNextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            prgCmds[0].cmdf = (int)Constants.MSOCMDF_ENABLED | (int)Constants.MSOCMDF_SUPPORTED;
            return VSConstants.S_OK;
        }

        public bool InSession
        {
            get
            {
                return _session != null;
            }
        }

        public int EndExpansion()
        {
            _session = null;
            _sessionEnded = true;
            _selectionStart = _selectionEnd = null;
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        private static string GetTemplateSelectionIndentation(string templateText, int selectedIndex)
        {
            string indentation = "";
            for (int i = selectedIndex - 1; i >= 0; i--)
            {
                if (templateText[i] != '\t' && templateText[i] != ' ')
                {
                    indentation = templateText.Substring(i + 1, selectedIndex - i - 1);
                    break;
                }
            }
            return indentation;
        }

        private string GetBaseIndentation(TextSpan[] ts)
        {
            var indentationLine = _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(ts[0].iStartLine).GetText();
            string baseIndentation = indentationLine;
            for (int i = 0; i < indentationLine.Length; i++)
            {
                if (indentationLine[i] != ' ' && indentationLine[i] != '\t')
                {
                    baseIndentation = indentationLine.Substring(0, i);
                    break;
                }
            }
            return baseIndentation;
        }

        private void IndentSpan(ITextEdit edit, string indentation, int startLine, int endLine)
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            for (int i = startLine; i <= endLine; i++)
            {
                var curline = snapshot.GetLineFromLineNumber(i);
                edit.Insert(curline.Start, indentation);
            }
        }

        public int GetExpansionFunction(IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc)
        {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath)
        {
            int caretLine, caretColumn;
            GetCaretPosition(out caretLine, out caretColumn);

            var textSpan = new TextSpan() { iStartLine = caretLine, iStartIndex = caretColumn, iEndLine = caretLine, iEndIndex = caretColumn };
            return InsertNamedExpansion(pszTitle, pszPath, textSpan);
        }

        public int InsertNamedExpansion(string pszTitle, string pszPath, TextSpan textSpan)
        {
            if (_session != null)
            {
                // if the user starts an expansion session while one is in progress
                // then abort the current expansion session
                _session.EndCurrentExpansion(1);
                _session = null;
            }

            var expansion = _lines as IVsExpansion;
            if (expansion == null) return VSConstants.S_OK;

            var selection = _textView.Selection;
            var snapshot = selection.Start.Position.Snapshot;

            _selectionStart = snapshot.CreateTrackingPoint(selection.Start.Position, PointTrackingMode.Positive);
            _selectionEnd = snapshot.CreateTrackingPoint(selection.End.Position, PointTrackingMode.Negative);
            _selectEndSpan = _sessionEnded = false;

            var hr = expansion.InsertNamedExpansion(
                pszTitle,
                pszPath,
                textSpan,
                this,
                new Guid(GuidList.PowerShellLanguage),
                0,
                out _session
                );

            if (ErrorHandler.Succeeded(hr))
            {
                if (_sessionEnded)
                {
                    _session = null;
                }
            }
            return VSConstants.S_OK;
        }

        public int NextField()
        {
            return _session.GoToNextExpansionField(0);
        }

        public int PreviousField()
        {
            return _session.GoToPreviousExpansionField();
        }

        private void GetCaretPosition(out int caretLine, out int caretColumn)
        {
            ErrorHandler.ThrowOnFailure(_view.GetCaretPos(out caretLine, out caretColumn));

            // Handle virtual space
            int lineLength;
            ErrorHandler.ThrowOnFailure(_lines.GetLengthOfLine(caretLine, out lineLength));

            if (caretColumn > lineLength)
            {
                caretColumn = lineLength;
            }
        }

        public int EndCurrentExpansion(bool leaveCaret)
        {
            if (_selectEndSpan)
            {
                TextSpan[] endSpan = new TextSpan[1];
                if (ErrorHandler.Succeeded(_session.GetEndSpan(endSpan)))
                {
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var startLine = snapshot.GetLineFromLineNumber(endSpan[0].iStartLine);
                    var span = new Span(startLine.Start + endSpan[0].iStartIndex, 4);
                    _textView.Caret.MoveTo(new SnapshotPoint(snapshot, span.Start));
                    _textView.Selection.Select(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, span), false);
                    return _session.EndCurrentExpansion(1);
                }
            }
            return _session.EndCurrentExpansion(leaveCaret ? 1 : 0);
        }
       
    }

    
}
