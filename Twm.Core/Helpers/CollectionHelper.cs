using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Twm.Core.Helpers
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Sorted add for observable collection using custom comparer
        /// </summary>
        public static void AddSorted<T>(this ObservableCollection<T> collection, T item, IComparer<T> comparer)
        {
            var sortableList = new List<T>(collection);
            int index = sortableList.BinarySearch(item, comparer);
            if (index < 0)
                index = ~index;
            collection.Insert(index, item);
        }
    }
}
