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

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace PowerShellTools.Intellisense
{
    [Export(typeof(IIntellisenseControllerProvider)), ContentType(PowerShellConstants.LanguageName), Order]
    internal class IntellisenseControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        public ICompletionBroker CompletionBroker = null; // Set via MEF

        [Import]
        public IVsEditorAdaptersFactoryService AdaptersFactory { get; set; }

        [Import]
        public ISignatureHelpBroker SigBroker = null; // Set via MEF

        [Import]
        public IQuickInfoBroker QuickInfoBroker = null; // Set via MEF

        [Import]
        public IIncrementalSearchFactoryService IncrementalSearch = null; // Set via MEF        

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            if (!textView.Properties.TryGetProperty(typeof(IntellisenseController), out IntellisenseController controller))
            {
                controller = new IntellisenseController(this, textView);
                controller.AttachKeyboardFilter();
            }
            return controller;
        }
    }

    /// <summary>
    /// Monitors creation of text view adapters for PowerShell code so that we can attach
    /// our keyboard filter.  This enables not using a keyboard pre-preprocessor
    /// so we can process all keys for text views which we attach to.  We cannot attach
    /// our command filter on the text view when our intellisense controller is created
    /// because the adapter does not exist.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class TextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;

        [ImportingConstructor]
        public TextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory)
        {
            _adaptersFactory = adaptersFactory;
        }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);
            IntellisenseController controller;
            if (textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller))
            {
                controller.AttachKeyboardFilter();
            }
        }

        #endregion
    }

}
