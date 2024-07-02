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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Implements the navigation bar which appears above a source file in the editor.
    /// The navigation bar consists of two drop-down boxes.  The left hand side is simply a static node for the script
    /// The right hand side is a list of function definitions in the script
    /// </summary>
    internal class DropDownBarClient : IVsDropdownBarClient, IDisposable
    {
        private ReadOnlyCollection<IDropDownEntryInfo> _topLevelEntries;
        private ReadOnlyCollection<IDropDownEntryInfo> _nestedEntries;
        private readonly Dispatcher _dispatcher;
        private readonly IWpfTextView _textView;
        private IVsDropdownBar _dropDownBar;
        private int _topLevelIndex = -1, _nestedIndex = -1;
        private static readonly ImageList _imageList = GetImageList();
        private IPowerShellTokenizationService _tokenizer;

        public DropDownBarClient(IWpfTextView textView)
        {
            Utilities.ArgumentNotNull("textView", textView);
            _textView = textView;

            if (_textView.TextBuffer.ContentType.IsOfType(PowerShellConstants.LanguageName))
            {
                _textView.TextBuffer.Properties.TryGetProperty(BufferProperties.PowerShellTokenizer, out _tokenizer);
                //if (_tokenizer == null)
                //    _tokenizer = new PowerShellTokenizationService(textView.TextBuffer);
            }
            else
            {
                return;
                //_tokenizer = new PowerShellTokenizationService(textView.TextBuffer);
            }

            if (_tokenizer == null) return;

            _dispatcher = Dispatcher.CurrentDispatcher;
            _textView.Caret.PositionChanged += Caret_PositionChanged;
            _tokenizer.TokenizationComplete += Tokenizer_TokenizationComplete;
        }

        /// <summary>
        /// Disposes handlers
        /// </summary>
        public void Dispose()
        {
            try
            {
                _textView.Caret.PositionChanged -= Caret_PositionChanged;
                _tokenizer.TokenizationComplete -= Tokenizer_TokenizationComplete;
            }
            catch { }
        }

        #region IVsDropdownBarClient Members

        /// <summary>
        /// Gets the attributes for the specified comboBox
        /// (text, image, and attributes of the text such as being grayed out)
        /// </summary>
        public int GetComboAttributes(int comboBoxId, out uint count, out uint attributes, out IntPtr imageList)
        {
            var entries = GetEntries(comboBoxId);
            if (entries != null)
            {
                count = (uint)entries.Count;
            }
            else
            {
                count = 0;
            }

            attributes = (uint)(DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_IMAGE | DROPDOWNENTRYTYPE.ENTRY_ATTR);
            imageList = _imageList.Handle;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the tool tip for the given combo box.
        /// </summary>
        public int GetComboTipText(int comboBoxId, out string toolTipText)
        {
            toolTipText = null;

            if (comboBoxId == ComboBoxId.Nested)
            {
                var index = GetSelectedIndex(comboBoxId);
                var entries = GetEntries(comboBoxId);
                if (entries != null && index != -1 && index < entries.Count)
                {
                    toolTipText = entries[index].DisplayText + "\n\n" + Resources.DropDownToolTip;
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the entry attributes for the given combo box and index.
        /// </summary>
        public int GetEntryAttributes(int comboBoxId, int index, out uint textColor)
        {
            textColor = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;

            var entries = GetEntries(comboBoxId);
            var selectedIndex = GetSelectedIndex(comboBoxId);
            var caretPosition = _textView.Caret.Position.BufferPosition.Position;
            if (entries != null && index < entries.Count &&
                index == selectedIndex &&
                (caretPosition < entries[selectedIndex].Start ||
                 caretPosition > entries[selectedIndex].End))
            {
                textColor = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the image for the given combo box and index.
        /// </summary>
        public int GetEntryImage(int comboBoxId, int index, out int piImageIndex)
        {
            piImageIndex = 0;

            var entries = GetEntries(comboBoxId);
            if (entries != null && index < entries.Count)
            {
                piImageIndex = entries[index].ImageListIndex;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the text displayed for the given combo box and index.
        /// </summary>
        public int GetEntryText(int comboBoxId, int index, out string displayText)
        {
            displayText = String.Empty;

            var entries = GetEntries(comboBoxId);
            if (entries != null && index < entries.Count)
            {
                displayText = entries[index].DisplayText;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when the user chooses an item from the drop down
        /// </summary>
        public int OnComboGetFocus(int comboBoxId)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when the user chooses an item from the drop down
        /// </summary>
        public int OnItemChosen(int comboBoxId, int index)
        {
            if (_dropDownBar == null)
            {
                return VSConstants.E_UNEXPECTED;
            }

            try
            {
                var entries = GetEntries(comboBoxId);
                if (entries != null && index < entries.Count)
                {
                    SetSelectedIndex(comboBoxId, index);
                    _dropDownBar.RefreshCombo(comboBoxId, index);

                    var functionEntryInfo = entries[index] as FunctionDefinitionEntryInfo;
                    if (functionEntryInfo != null)
                    {
                        NavigationExtensions.NavigateToFunctionDefinition(_textView, functionEntryInfo.FunctionDefinition);
                    }
                    else
                    {
                        NavigationExtensions.NavigateToLocation(_textView, entries[index].Start);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger(typeof(DropDownBarClient));
                logger.Error("Failed to navigate to definition", ex);
                MessageBox.Show("There was an error navigating to the definition.", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when the user selects an item from the drop down
        /// </summary>
        public int OnItemSelected(int comboBoxId, int index)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by VS to provide us with the drop down bar
        /// </summary>
        public int SetDropdownBar(IVsDropdownBar dropDownBar)
        {
            _dropDownBar = dropDownBar;

            ParseError[] errors;
            Token[] tokens;
            var ast = Parser.ParseInput(_textView.TextBuffer.CurrentSnapshot.GetText(), out tokens, out errors);
            UpdateDropDownEntries(ast);

            return VSConstants.S_OK;
        }

        #endregion

        #region Selection Synchronization

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            Action callback = () => {
                // At the moment, the topLevel drop down box never changes, so we only update the nested drop down box
                SetActiveSelection(ComboBoxId.Nested);
            };
            _dispatcher.BeginInvoke(callback, DispatcherPriority.Background);
        }

        private void Tokenizer_TokenizationComplete(object sender, Ast ast)
        {
            if (_dropDownBar != null)
            {
                Action callback = () => {
                    UpdateDropDownEntries(ast);
                };
                _dispatcher.BeginInvoke(callback, DispatcherPriority.Background);
            }
        }

        private void UpdateDropDownEntries(Ast ast)
        {
            _topLevelEntries = CalculateTopLevelEntries(ast);
            SetActiveSelection(ComboBoxId.TopLevel);
            _nestedEntries = CalculateNestedEntries(ast);
            SetActiveSelection(ComboBoxId.Nested);
        }

        private void SetActiveSelection(int comboBoxId)
        {
            if (_dropDownBar != null)
            {
                var newIndex = -1;

                var entries = GetEntries(comboBoxId);
                if (entries != null && entries.Any())
                {
                    var newPosition = _textView.Caret.Position.BufferPosition.Position;
                    var entriesByScope = entries.OrderBy(entry => entry.End);
                    var activeEntry = entriesByScope.FirstOrDefault(entry => newPosition >= entry.Start && newPosition <= entry.End);

                    if (activeEntry != null)
                    {
                        newIndex = entries.IndexOf(activeEntry);
                    }
                    else
                    {
                        // If outside all entries, select the entry just before it
                        var closestEntry = entriesByScope.LastOrDefault(entry => newPosition >= entry.End);
                        if (closestEntry == null)
                        {
                            // if the mouse is before any entries, select the first one
                            closestEntry = entries.OrderBy(entry => entry.Start).First();
                        }

                        newIndex = entries.IndexOf(closestEntry);
                    }
                }

                SetSelectedIndex(comboBoxId, newIndex);
                _dropDownBar.RefreshCombo(comboBoxId, newIndex);
            }
        }

        #endregion

        #region Entry Calculation

        /// <summary>
        /// Reads our image list from our DLLs resource stream.
        /// </summary>
        private static ImageList GetImageList()
        {
            ImageList list = new ImageList();
            list.ImageSize = new Size(0x10, 0x10);
            list.TransparentColor = Color.FromArgb(0xff, 0, 0xff);
            Stream manifestResourceStream = typeof(DropDownBarClient).Assembly.GetManifestResourceStream("PowerShellTools.Resources.completionset.bmp");
            list.Images.AddStrip(new Bitmap(manifestResourceStream));
            return list;
        }

        private static ReadOnlyCollection<IDropDownEntryInfo> CalculateTopLevelEntries(Ast script)
        {
            var newEntries = new Collection<IDropDownEntryInfo>();

            if (script != null)
            {
                newEntries.Add(new ScriptEntryInfo(script));
            }

            return new ReadOnlyCollection<IDropDownEntryInfo>(newEntries);
        }

        private static ReadOnlyCollection<IDropDownEntryInfo> CalculateNestedEntries(Ast script)
        {
            List<IDropDownEntryInfo> newEntries = new List<IDropDownEntryInfo>();

            if (script != null)
            {
                foreach (var function in script.FindAll(node => node is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>())
                {
                    newEntries.Add(new FunctionDefinitionEntryInfo(function));
                }
            }

            newEntries.Sort((x, y) => string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase));
            return new ReadOnlyCollection<IDropDownEntryInfo>(newEntries);
        }

        private static class ComboBoxId
        {
            public const int TopLevel = 0;
            public const int Nested = 1;
        }

        private ReadOnlyCollection<IDropDownEntryInfo> GetEntries(int comboBoxId)
        {
            switch (comboBoxId)
            {
                case ComboBoxId.TopLevel:
                    return _topLevelEntries;
                case ComboBoxId.Nested:
                    return _nestedEntries;
                default:
                    return null;
            }
        }

        private int GetSelectedIndex(int comboBoxId)
        {
            switch (comboBoxId)
            {
                case ComboBoxId.TopLevel:
                    return _topLevelIndex;
                case ComboBoxId.Nested:
                    return _nestedIndex;
                default:
                    return -1;
            }
        }

        private void SetSelectedIndex(int comboBoxId, int index)
        {
            switch (comboBoxId)
            {
                case ComboBoxId.TopLevel:
                    _topLevelIndex = index;
                    break;
                case ComboBoxId.Nested:
                    _nestedIndex = index;
                    break;
            }
        }
        #endregion
    }
}
