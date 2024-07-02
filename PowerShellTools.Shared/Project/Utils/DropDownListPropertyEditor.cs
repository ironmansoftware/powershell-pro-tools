//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2009 CodeSmith Tools, LLC.  All rights reserved.
// 
// The terms of use for this software are contained in the file
// named sourcelicense.txt, which can be found in the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by the
// terms of this license.
// 
// You must not remove this notice, or any other, from this software.
//
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using PowerShellTools.Project.Utils;

namespace PowerGUIVsx.Project.Utils
{
    /// <summary>
    /// Provides a user interface for selecting a state property.
    /// </summary>
    public class DropDownListPropertyEditor : UITypeEditor
    {
        #region Members

        private IWindowsFormsEditorService _service = null;

        #endregion

        /// <summary>
        /// Displays a list of available values for the specified component than sets the value.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services may be obtained.</param>
        /// <param name="value">An instance of the value being edited.</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed, this method should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                // This service is in charge of popping our ListBox.
                _service = ((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService)));

                if (_service != null && value is DropDownListProperty)
                {
                    var property = (DropDownListProperty) value;

                    var list = new ListBox();
                    list.Click += ListBox_Click;

                    foreach (string item in property.Values)
                    {
                        list.Items.Add(item);
                    }

                    // Drop the list control.
                    _service.DropDownControl(list);

                    if (list.SelectedItem != null && list.SelectedIndices.Count == 1)
                    {
                        property.SelectedItem = list.SelectedItem.ToString();
                        value =  property;
                    }
                }
            }

            return value;
        }

        private void ListBox_Click(object sender, EventArgs e)
        {
            if(_service != null)
                _service.CloseDropDown();
        }

        /// <summary>
        /// Gets the editing style of the <see cref="EditValue"/> method.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <returns>Returns the DropDown style, since this editor uses a drop down list.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // We're using a drop down style UITypeEditor.
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
