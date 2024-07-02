using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellTools.Explorer
{
    internal static class Extensions
    {
        /// <summary>
        /// Adds a range of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observableCollection"></param>
        /// <param name="items">The range to add.</param>
        /// <param name="clear">If true the collection is cleared
        /// before the range of new items is added.</param>
        internal static void AddItems<T>(this ObservableCollection<T> observableCollection, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                observableCollection.Clear();
            }

            foreach (T item in items) observableCollection.Add(item);
        }

        /// <summary>
        /// Adds a range of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items">The range to add.</param>
        /// <param name="clear">If true the collection is cleared
        /// before the range of new items is added.</param>
        internal static void AddItems<T>(this List<T> list, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                list.Clear();
            }

            list.AddRange(items);
        }

        /// <summary>
        /// Adds a range of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items">The range to add.</param>
        /// <param name="clear">If true the collection is cleared
        /// before the range of new items is added.</param>
        internal static void AddItems<T>(this ObservableList<T> list, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                list.Clear();
            }

            list.AddRange(items);
        }
    }
}
