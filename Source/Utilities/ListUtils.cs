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
        
    }
}
