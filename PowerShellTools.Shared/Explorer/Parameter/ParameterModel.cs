using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    [DebuggerDisplay("{Name}:{Type}")]
    internal class ParameterModel : ObservableObject, INotifyDataErrorInfo
    {
        private string _value =  string.Empty;

        public ParameterModel(string set, string name, ParameterType type, bool isMandatory, string helpMesssage)
        {
            Set = set;
            Name = name;
            Type = type;
            IsMandatory = isMandatory;
            HelpMessage = string.IsNullOrWhiteSpace(helpMesssage) ? Name : helpMesssage;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public string Set { get; private set; }
        public string Name { get; private set; }
        public ParameterType Type { get; private set; }
        public bool IsMandatory { get; private set; }
        public string HelpMessage { get; private set; }
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged();
                    RaiseErrorsChanged();
                }
            }
        }

        public virtual bool HasErrors
        {
            get
            {
                return IsMandatory && string.IsNullOrWhiteSpace(_value);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            yield return propertyName;
        }

        private void RaiseErrorsChanged()
        {
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if(handler != null)
            {
                handler(this, new DataErrorsChangedEventArgs("Value"));
            }
        }
    }
}
