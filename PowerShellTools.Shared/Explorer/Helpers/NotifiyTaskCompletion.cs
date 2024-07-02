using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal sealed class NotifiyTaskCompletion<TResult> : INotifyPropertyChanged
    {
        public NotifiyTaskCompletion(Task<TResult> task)
        {
            Task = task;
            if (!task.IsCompleted & !task.IsCompleted)
            {
                var t = ExecuteTaskAsync(task);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Task<TResult> Task { get; private set; }

        public TResult Result
        {
            get
            {
                return (Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
            }
        }

        public TaskStatus Status
        {
            get
            {
                return Task.Status;
            }
        }

        public AggregateException Exception
        {
            get
            {
                return Task.Exception;
            }
        }

        private async Task ExecuteTaskAsync(Task task)
        {
            await task;
        }

        private void RaisePropertyChanged(string property)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
