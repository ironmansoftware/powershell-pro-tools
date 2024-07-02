using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal class ParameterEditorViewModel : ViewModel
    {
        private readonly IDialog _window;
        private readonly IDataProvider _dataProvider;
        private readonly string _title;

        private IPowerShellCommand _commandInfo;
        private CommandModel _commandModel;
        private string _commandPreview = string.Empty;
        private int _selectedIndex = 0;
        private string _selectedItem = string.Empty;
        private bool _isBusy;

        public ParameterEditorViewModel(IDialog window, IDataProvider dataProvider, IPowerShellCommand commandInfo)
        {
            _window = window;
            _dataProvider = dataProvider;
            _commandInfo = commandInfo;
            _isBusy = true;

            _title = string.Format("Parameters: {0}", _commandInfo.Name);
            _dataProvider.GetCommandMetaData(_commandInfo, GetCommandMetadataCallback);

            ShowDetailsCommand = new ViewModelCommand<object>(this, ShowDetails, CanShowDetails);
            Close = new ViewModelCommand(this, _window.Close);
        }

        public ViewModelCommand<object> ShowDetailsCommand { get; set; }
        public ViewModelCommand Close { get; set; }

        public string Title
        {
            get
            {
                return _title;
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IPowerShellCommand Info
        {
            get
            {
                return _commandInfo;
            }

            set
            {
                if (_commandInfo != value)
                {
                    _commandInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CommandModel Model
        {
            get
            {
                return _commandModel;
            }

            set
            {
                if (_commandModel != value)
                {
                    _commandModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CommandPreview
        {
            get
            {
                return _commandPreview;
            }

            set
            {
                if (_commandPreview != value)
                {
                    _commandPreview = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                this.Model.SelectParameterSetByName(_selectedItem);
                UpdateCommandPreview();
            }
        }

        public void ShowDetails(object parameter)
        {
            var window = new PSCommandDetails(_dataProvider, _commandInfo);
            window.Show();
        }

        private bool CanShowDetails(object parameter)
        {
            return _commandInfo != null;
        }

        private void GetCommandMetadataCallback(IPowerShellCommandMetadata result)
        {
            Model = CommandModelFactory.GenerateCommandModel(result);

            if (Model != null)
            {
                UpdateCommandPreview();
                Model.PropertyChanged += OnCommandModelPropertyChanged;
            }

            IsBusy = false;
        }

        private void OnCommandModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCommandPreview();
        }

        private void UpdateCommandPreview()
        {
            CommandPreview = string.Empty;
        }
    }
}
