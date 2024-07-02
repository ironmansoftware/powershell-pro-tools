using System.Windows;

namespace PowerShellTools.Editors.ViewModel
{
    class HeaderViewModel : ViewModelBase
    {
        private Visibility _visibility;

        public HeaderViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }

        public MainWindowViewModel MainWindowViewModel { get; set; }

        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged("Visibility"); }
        }



    }
}
