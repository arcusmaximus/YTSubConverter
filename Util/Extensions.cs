using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static int BinarySearchIndexAtOrAfter<T>(this IList<T> items, T searchItem)
        {
            int startIdx = 0;
            int endIdx = items.Count;
            while (endIdx > startIdx)
            {
                int pivotIdx = (startIdx + endIdx) / 2;
                T pivotItem = items[pivotIdx];
                int comparison = Comparer<T>.Default.Compare(searchItem, pivotItem);
                if (comparison == 0)
                    return pivotIdx;

                if (comparison < 0)
                    endIdx = pivotIdx;
                else
                    startIdx = pivotIdx + 1;
            }
            return startIdx;
        }

        public static IEnumerable<IGrouping<TKey, TItem>> GroupByContiguous<TKey, TItem>(this IEnumerable<TItem> items, Func<TItem, TKey> keySelector)
        {
            Grouping<TKey, TItem> currentGroup = null;
            foreach (TItem item in items)
            {
                TKey key = keySelector(item);
                if (currentGroup != null && !EqualityComparer<TKey>.Default.Equals(key, currentGroup.Key))
                {
                    yield return currentGroup;
                    currentGroup = null;
                }

                if (currentGroup == null)
                    currentGroup = new Grouping<TKey, TItem>(key);

                currentGroup.Items.Add(item);
            }
            if (currentGroup != null)
                yield return currentGroup;
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

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            foreach (KeyValuePair<TKey, TValue> pair in pairs)
            {
                dict.Add(pair);
            }
        }

        public static int RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, bool> predicate)
        {
            List<TKey> keys = dict.Keys.Where(predicate).ToList();
            foreach (TKey key in keys)
            {
                dict.Remove(key);
            }
            return keys.Count;
        }

        private class Grouping<TKey, TItem> : IGrouping<TKey, TItem>
        {
            public Grouping(TKey key)
            {
                Key = key;
                Items = new List<TItem>();
            }

            public TKey Key
            {
                get;
            }

            public List<TItem> Items
            {
                get;
            }

            public IEnumerator<TItem> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
