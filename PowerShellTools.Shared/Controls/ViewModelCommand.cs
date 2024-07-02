using System;
using System.Windows.Input;

namespace PowerShellTools.Common.Controls
{
    public sealed class ViewModelCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Predicate<object> _canExecute;

        public ViewModelCommand(Action executeAction)
            : this(executeAction != null ? _ => executeAction() : (Action<object>)null, (Predicate<object>)null)
        {
        }

        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecute = null)
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException("executeAction");
            }

            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        #endregion

        public void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
