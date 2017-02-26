using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCode2017
{
    public static class ListHelper
    {
        public static KeyValuePair<TKey, TValue> GetAndRemoveLast<TKey, TValue>(this SortedList<TKey, TValue> list)
        {
            KeyValuePair<TKey, TValue> result = list.ElementAt(list.Count - 1);
            list.RemoveAt(list.Count - 1);
            return result;
        }
    }
}
