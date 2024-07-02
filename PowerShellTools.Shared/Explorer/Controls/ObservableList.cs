using System.Collections.Generic;
using System.Collections.Specialized;

namespace PowerShellTools.Explorer
{
    internal class ObservableList<T> : List<T>, INotifyCollectionChanged
    {
        /// <summary>
        /// Adds a single item to the observable list and raises a
        /// collection changed event.
        /// </summary>
        /// <param name="item"></param>
        public new void Add(T item)
        {
            base.Add(item);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        }

        /// <summary>
        /// Adds a range of items to the observable list and raises a
        /// collection changed event when all items within the range
        /// have been added to the observable list.
        /// </summary>
        /// <param name="items"></param>
        public new void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items) { base.Add(item); }
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Clears all items from the observable list and raises a
        /// collection changed event when all items are removed.
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedAction a)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(a));
            }
        }
    }
}
