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
using System.Collections.Generic;
using System.ComponentModel;
using PowerGUIVsx.Project.Utils;

namespace PowerShellTools.Project.Utils
{
    [Editor(typeof(DropDownListPropertyEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class DropDownListProperty
    {
        private List<string> _values = new List<string>();

        public DropDownListProperty()
        {
            SelectedItem = "None";
        }

        public DropDownListProperty(List<String> values)
        {
            if(values.Count > 0)
                SelectedItem = values[0];
            else
                SelectedItem = "None";

            Values = values;
        }

        public List<string> Values 
        { 
            get
            {
                if (_values == null)
                    _values = new List<String>();

                return _values;
            }
            set
            {
                if(value != null)
                    _values = value;
            }
        }

        [Browsable(false)]
        public string SelectedItem { get; set; }

        /// <summary>
        /// The value that we return here will be shown in the property grid.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SelectedItem;
        }
    }
}
