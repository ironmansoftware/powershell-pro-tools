namespace PowerShellTools.Editors.ViewModel
{
    class DescriptionViewModel : ViewModelBase
    {
        private string _shortDescription;
        private string _longDescription;
        private string _notes;
        private string _inputType;
        private string _inputTypeDescription;
        private string _outputType;
        private string _outputTypeDescription;

        public string LongDescription
        {
            get { return _longDescription; }
            set { _longDescription = value; OnPropertyChanged("LongDescription"); }
        }

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; OnPropertyChanged("Notes"); }
        }

        public string InputType
        {
            get { return _inputType; }
            set { _inputType = value; OnPropertyChanged("InputType"); }
        }

        public string InputTypeDescription
        {
            get { return _inputTypeDescription; }
            set { _inputTypeDescription = value; OnPropertyChanged("InputTypeDescription"); }
        }

        public string OutputType
        {
            get { return _outputType; }
            set { _outputType = value; OnPropertyChanged("OutputType"); }
        }

        public string OutputTypeDescription
        {
            get { return _outputTypeDescription; }
            set { _outputTypeDescription = value; OnPropertyChanged("OutputTypeDescription"); }
        }


        public string ShortDescription
        {
            get { return _shortDescription; }
            set { _shortDescription = value; OnPropertyChanged("ShortDescription"); }
        }
    }
}
