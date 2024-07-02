using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.FormDesigner
{
    public class FormTab
    {
        public FormTab()
        {
            FormFields = new List<FormField>();
        }

        public string Name { get; set; }
        public List<FormField> FormFields { get; set; }
    }

    public class FormField
    {
        public bool Required { get; set; }

     
        public FieldTypes Type { get; set; }

     
        public Type DotNetType { get; set; }

     
        public object[] ValidOptions { get; set; }
     
        public string[] Placeholder { get; set; }

     
        public string Name { get; set; }

     
        public object Value { get; set; }
        
        public string ValidationErrorMessage { get; set; }
        
    }

    public enum FieldTypes
    {
        Checkbox,
        Textbox,
        Select,
        Date,
        Time
    }
}
