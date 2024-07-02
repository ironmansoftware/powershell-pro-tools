using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;

namespace PowerShellTools.CredentialUI
{
    /// <summary>
    /// View model of SecureString prompt dialog
    /// </summary>
    public class SecureStringDialogViewModel : INotifyPropertyChanged
    {
        private SecureString _secString;
        private string _parameterName;
        private string _parameterMessage;

        public SecureStringDialogViewModel(string paramMessage, string paramName)
        {
            _parameterMessage = paramMessage;
            _parameterName = paramName;
        }

        /// <summary>
        /// Message of parameter
        /// </summary>
        public string ParameterMessage
        {
            get
            {
                return _parameterMessage;
            }
        }

        /// <summary>
        /// Name of parameter
        /// </summary>
        public string ParameterName
        {
            get
            {
                return _parameterName;
            }
        }

        /// <summary>
        /// SecureString get from password box
        /// </summary>
        public SecureString SecString
        {
            get
            {
                return _secString;
            }
            set
            {
                _secString = value;
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
