using System;
using System.Collections.Generic;
using System.Linq;

namespace _Scripts.Utils
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> ts) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
        
        public static bool TryGetFirst<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, bool> predicate, out TSource first)
        {
            first = default(TSource);
            if (enumerable == null) return false;
            first = enumerable.FirstOrDefault(predicate);
            return first != null && !first.Equals(default(TSource));
        }
    }
}