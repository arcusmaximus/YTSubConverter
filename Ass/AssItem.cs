using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Ass
{
    internal struct AssItem
    {
        public AssItem(AssSection section, string type, List<string> values)
        {
            Section = section;
            Type = type;
            Values = values;
        }

        public AssSection Section
        {
            get;
        }

        public string Type
        {
            get;
        }

        public List<string> Values
        {
            get;
        }

        public string GetString(string field)
        {
            return Values[Section.Format[field]];
        }

        public int GetInt(string field)
        {
            return int.Parse(GetString(field));
        }

        public float GetFloat(string field)
        {
            return float.Parse(GetString(field), CultureInfo.InvariantCulture);
        }

        public bool GetBool(string field)
        {
            return GetInt(field) != 0;
        }

        public Color GetColor(string field)
        {
            string value = GetString(field);
            if (value.Length != 10 || !value.StartsWith("&H"))
                throw new FormatException($"{value} is not a valid color");

            if (!uint.TryParse(value.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint abgr))
                throw new FormatException($"{value} is not a valid color");

            byte a = (byte)(255 - (abgr >> 24));
            byte r = (byte)abgr;
            byte g = (byte)(abgr >> 8);
            byte b = (byte)(abgr >> 16);
            return Color.FromArgb(a, r, g, b);
        }

        public DateTime GetTimestamp(string field)
        {
            string value = GetString(field);
            Match match = Regex.Match(value, @"^(\d+):(\d\d):(\d\d)\.(\d\d)$");
            if (!match.Success)
                throw new FormatException($"{value} is not a valid timestamp");

            return new DateTime(
                SubtitleDocument.TimeBase.Year,
                SubtitleDocument.TimeBase.Month,
                SubtitleDocument.TimeBase.Day,
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                int.Parse(match.Groups[4].Value) * 10
            );
        }
    }
}
