/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.Intellisense
{

    internal sealed class IntellisenseController : IIntellisenseController, IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly IntellisenseControllerProvider _provider;
        private readonly IntelliSenseManager _intelliSenseManager;

        /// <summary>
        /// Attaches events for invoking Statement completion 
        /// </summary>
        public IntellisenseController(IntellisenseControllerProvider provider, ITextView textView)
        {
            _textView = textView;
            _provider = provider;
            textView.Properties.AddProperty(typeof(IntellisenseController), this);  // added so our key processors can get back to us

            _intelliSenseManager = new IntelliSenseManager(provider.CompletionBroker, provider.ServiceProvider, null, textView);
        }

        public ICompletionBroker CompletionBroker
        {
            get
            {
                return _provider.CompletionBroker;
            }
        }

        public IVsEditorAdaptersFactoryService AdaptersFactory
        {
            get
            {
                return _provider.AdaptersFactory;
            }
        }

        public ISignatureHelpBroker SignatureBroker
        {
            get
            {
                return _provider.SigBroker;
            }
        }

        // we need this because VS won't give us certain keyboard events as they're handled before our key processor.  These
        // include enter and tab both of which we want to complete.
        public void AttachKeyboardFilter()
        {
            if (_intelliSenseManager.NextCommandHandler == null)
            {
                var viewAdapter = AdaptersFactory.GetViewAdapter(_textView);
                if (viewAdapter != null)
                {
                    IOleCommandTarget next;

                    ErrorHandler.ThrowOnFailure(viewAdapter.AddCommandFilter(this, out next));
                    _intelliSenseManager.NextCommandHandler = next;
                }
            }
        }

        #region IIntellisenseController implementation

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        /// <summary>
        /// Detaches the events
        /// </summary>
        /// <param name="textView">The text view to detach from.</param>
        public void Detach(ITextView textView)
        {
            if (_textView == null)
            {
                throw new InvalidOperationException("Already detached from text view");
            }
            if (textView != _textView)
            {
                throw new ArgumentException("Not attached to specified text view", "textView");
            }
            _textView.Properties.RemoveProperty(typeof(IntellisenseController));

            DetachKeyboardFilter();
        }

        #endregion

        #region IOleCommandTarget implementation

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return _intelliSenseManager.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return _intelliSenseManager.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        #endregion

        private void DetachKeyboardFilter()
        {
            var viewAdapter = AdaptersFactory.GetViewAdapter(_textView);

            if (_intelliSenseManager.NextCommandHandler != null)
            {
                ErrorHandler.ThrowOnFailure(viewAdapter.RemoveCommandFilter(this));
                _intelliSenseManager.NextCommandHandler = null;
            }
        }
    }
}

