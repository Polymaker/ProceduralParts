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
            //var comparer = new GenericComparer<T>(comparator);

            //return collection.Distinct(comparer);
        }

        private class GenericComparer<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> compPredicate;

            public GenericComparer(Func<T, T, bool> compPredicate)
            {
                this.compPredicate = compPredicate;
            }

            public bool Equals(T x, T y)
            {
                return compPredicate(x, y);
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

        }
    }
}
