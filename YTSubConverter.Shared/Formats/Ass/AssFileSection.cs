using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Arc.YTSubConverter.Shared.Formats.Ass
{
    internal class AssDocumentSection
    {
        public Dictionary<string, int> Format
        {
            get;
            set;
        }

        public List<AssDocumentItem> Items { get; } = new List<AssDocumentItem>();

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
            Items.Add(new AssDocumentItem(this, type, values));
        }

        public string GetItemString(string type)
        {
            return Items.FirstOrDefault(i => i.Type == type).Values?.FirstOrDefault();
        }

        public int GetItemInt(string type, int defaultValue = 0)
        {
            return int.TryParse(GetItemString(type), out int result) ? result : defaultValue;
        }

        public float GetItemFloat(string type, float defaultValue = 0)
        {
            return float.TryParse(GetItemString(type), NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        public bool GetItemBool(string type, bool defaultValue = false)
        {
            return Convert.ToBoolean(GetItemInt(type, Convert.ToInt32(defaultValue)));
        }

        public IEnumerable<T> MapItems<T>(string type, Func<AssDocumentItem, T> selector)
        {
            return Items.Where(i => i.Type == type).Select(selector);
        }
    }
}
