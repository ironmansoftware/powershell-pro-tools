using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerShellTools.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Sets the given property name (automatically passed in by compiler in 
        /// most cases) to the given value, if it has changed, and sends a property
        /// changed notification.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="backingProperty">The backing field for the property</param>
        /// <param name="value">The new property value</param>
        /// <param name="propertyName">The property name</param>
        protected void SetProperty<T>(ref T backingProperty, T value, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(backingProperty, value))
            {
                backingProperty = value;
                NotifyPropertyChanged(propertyName);
            }
        }
    }
}
