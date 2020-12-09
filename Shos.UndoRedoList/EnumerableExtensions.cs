using System;
using System.Collections.Generic;
using System.Linq;

namespace Shos.Collections
{
    /// <summary>Extension methods for IEnumerable<TElement>.</summary>
    public static class EnumerableExtensions
    {
        public static void ForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
        {
            foreach (var element in @this)
                action(element);
        }

        public static void ReverseForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
        {
            var list = @this as IList<TElement>;
            if (list is null)
                list = @this.ToList();

            for (var index = list.Count - 1; index >= 0; index--)
                action(list[index]);
        }

    }
}
