using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralParts
{
    internal static class ListUtils
    {
        public static IEnumerable<T> RemoveDoubles<T>(this IEnumerable<T> collection, Func<T, T , bool> comparator)
        {
            var elements = collection.ToList();
            var final = new List<T>();
            while (elements.Count > 0)
            {
                final.Add(elements[0]);
                elements.RemoveAll(e => comparator(elements[0], e));
            }
            return final;
        }

        public static void OrderListBy<TSource, TKey>(this List<TSource> list, Func<TSource, TKey> selector)
        {
            var orderedList = list.OrderBy(selector).ToList();
            list.Clear();
            list.AddRange(orderedList);
        }

        public static void Remove<T>(this List<T> list, IEnumerable<T> items)
        {
            var elements = items.ToArray();
            for (int i = 0; i < elements.Length; i++)
                list.Remove(elements[i]);
        }
    }
}
