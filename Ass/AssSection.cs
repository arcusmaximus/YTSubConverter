using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc.YTSubConverter.Ass
{
    internal class AssSection
    {
        public Dictionary<string, int> Format
        {
            get;
            set;
        }

        public List<AssItem> Items { get; } = new List<AssItem>();

        public void SetFormat(List<string> format)
        {
            Format = new Dictionary<string, int>();
            for (int i = 0; i < format.Count; i++)
            {
                Format.Add(format[i], i);
            }
        }

        public void AddItem(string type, List<string> values)
        {
            Items.Add(new AssItem(this, type, values));
        }

        public IEnumerable<T> MapItems<T>(string type, Func<AssItem, T> selector)
        {
            return Items.Where(i => i.Type == type).Select(selector);
        }
    }
}
