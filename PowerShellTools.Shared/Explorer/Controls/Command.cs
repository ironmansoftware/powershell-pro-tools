using System;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    internal abstract class Command : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add 
            { 
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.CanExecuteInternal(parameter);
        }

        public void Execute(object parameter)
        {
            this.ExecuteInternal(parameter);
        }

        protected abstract bool CanExecuteInternal(object parameter);

        protected abstract void ExecuteInternal(object parameter);
    }
}
