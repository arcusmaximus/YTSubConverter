using System;
using System.Collections.Generic;

namespace Arc.YTSubConverter.Util
{
    internal static class Extensions
    {
        public static List<string> Split(this string str, string separator, int? maxItems = null)
        {
            List<string> result = new List<string>();
            int start = 0;
            int end;
            while (start <= str.Length)
            {
                if (start < str.Length && (maxItems == null || result.Count < maxItems.Value - 1))
                {
                    end = str.IndexOf(separator, start);
                    if (end < 0)
                        end = str.Length;
                }
                else
                {
                    end = str.Length;
                }

                result.Add(str.Substring(start, end - start));
                start = end + separator.Length;
            }
            return result;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T itemToSearch, IEqualityComparer<T> comparer)
        {
            int index = 0;
            foreach (T item in items)
            {
                if (comparer.Equals(item, itemToSearch))
                    return index;

                index++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in items)
            {
                if (predicate(item))
                    return index;

                index++;
            }
            return -1;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryGetValue(key, out TValue value);
            return value;
        }

        public static TValue FetchValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> getValue)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = getValue();
                dict.Add(key, value);
            }
            return value;
        }
    }
}
