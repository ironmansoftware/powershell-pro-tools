using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Repl.DialogWindows
{
    class RemoteSessionWindowViewModel : INotifyPropertyChanged
    {
        private string _computerName;

        /// <summary>
        /// Selected template item
        /// </summary>
        public string ComputerName
        {
            get
            {
                return _computerName;
            }
            set
            {
                if (_computerName != value)
                {
                    _computerName = value;

                    OnPropertyChanged("ComputerName");
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raised when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
