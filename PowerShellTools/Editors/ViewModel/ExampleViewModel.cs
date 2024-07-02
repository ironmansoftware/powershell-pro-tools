namespace PowerShellTools.Editors.ViewModel
{
    class ExampleViewModel : ViewModelBase
    {
        private Example _selectedExample;

        public Example SelectedExample
        {
            get { return _selectedExample; }
            set { _selectedExample = value; OnPropertyChanged("SelectedExample"); }
        }
    }

    class Example : ViewModelBase
    {
        private string _description;
        private string _name;
        private string _command;
        private string _output;

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        public string Command
        {
            get { return _command; }
            set { _command = value; OnPropertyChanged("Command"); }
        }

        public string Output
        {
            get { return _output; }
            set { _output = value; OnPropertyChanged("Output"); }
        }
    }
}
