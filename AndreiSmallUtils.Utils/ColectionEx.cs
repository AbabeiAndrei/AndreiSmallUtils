using System;
using System.Collections.Generic;

namespace AndreiSmallUtils.Utils
{
    public static class ColectionEx
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));

            return new List<T>(source);
        }

        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));

            return new List<T>(source);
        }
    }
}
