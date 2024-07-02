using System;
using System.ComponentModel;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    internal class ViewModelCommand : ICommand
    {
        private readonly ViewModel _vm;
        private readonly Action _execute = null;

        public ViewModelCommand(ViewModel vm, Action execute)
        {
            _vm = vm;
            _vm.PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        private void RaiseCanExecuteChanged()
        {
            EventHandler h = CanExecuteChanged;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaiseCanExecuteChanged();
        }
    }

    internal class ViewModelCommand<T> : ICommand
    {
        private readonly ViewModel _vm;
        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        public ViewModelCommand(ViewModel vm, Action<T> execute)
            :this(vm, execute, null)
        {
        }

        public ViewModelCommand(ViewModel vm, Action<T> execute, Predicate<T> canExecute)
        {
            _vm = vm;
            _vm.PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute != null ? _canExecute((T)parameter) : true;
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute((T)parameter);
            }
        }

        private void RaiseCanExecuteChanged()
        {
            EventHandler h = CanExecuteChanged;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaiseCanExecuteChanged();
        }
    }
}
