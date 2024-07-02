using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;
using PowerShellTools.Common.Logging;

namespace PowerShellTools.Explorer
{
    internal class PSCommandDetailsViewModel : ViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PSCommandDetailsViewModel));

        private readonly IDialog _window;
        private readonly IDataProvider _dataProvider;
        private readonly string _title;

        private IPowerShellCommand _commandInfo;
        private string _helpText;
        private bool _isBusy;

        public PSCommandDetailsViewModel(IDialog window, IDataProvider dataProvider, IPowerShellCommand commandInfo)
        {
            _window = window;
            _dataProvider = dataProvider;
            _commandInfo = commandInfo;
            _isBusy = true;

            _title = string.Format("Details: {0}", _commandInfo.Name);
            _dataProvider.GetCommandHelp(_commandInfo, GetHelpCallback);

            Close = new ViewModelCommand(this, _window.Close);
        }

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

        public string HelpText
        {
            get
            {
                return _helpText;
            }

            set
            {
                if (_helpText != value)
                {
                    _helpText = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void GetHelpCallback(string result)
        {
            HelpText = result;
            IsBusy = false;
        }
    }
}
