using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCode2017
{
    public static class ListHelper
    {
        public static T GetAndRemoveLast<T>(this List<T> list)
        {
            var result = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return result;
        }

        public static void AddSorted<T>(this List<T> list, T item, IComparer<T> comparer)
        {
            var targetIndex = list.SearchSortedNewPosition(item, comparer);
            list.Insert(targetIndex, item);
        }

        public static int SearchSortedNewPosition<T>(this List<T> list, T item, IComparer<T> comparer)
        {
            // stollen from System.Collections.Generic.ArraySortHelper<T>.BinarySearch
            int lo = 0;
            int hi = list.Count - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order;
                order = comparer.Compare(list[i], item);

                if (order == 0) return i;
                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return lo;
        }
    }
}
