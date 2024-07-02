using System.ComponentModel;

namespace PowerShellTools
{
    class PackageProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _enableDiagnosticLogging;
        public bool EnableDiagnosticLogging
        {
            get { return _enableDiagnosticLogging; }
            set { _enableDiagnosticLogging = value; OnPropertyChanged("EnableDiagnosticLogging"); }
        }
    }
}
