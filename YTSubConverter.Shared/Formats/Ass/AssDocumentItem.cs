using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats.Ass
{
    internal struct AssDocumentItem
    {
        public AssDocumentItem(AssDocumentSection section, string type, List<string> values)
        {
            Section = section;
            Type = type;
            Values = values;
        }

        public AssDocumentSection Section
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

        public int GetInt(string field, int defaultValue = 0)
        {
            return int.TryParse(GetString(field), out int result) ? result : defaultValue;
        }

        public float GetFloat(string field, float defaultValue = 0)
        {
            return float.TryParse(GetString(field), NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        public bool GetBool(string field, bool defaultValue = false)
        {
            return Convert.ToBoolean(GetInt(field, Convert.ToInt32(defaultValue)));
        }

        public Color GetColor(string field)
        {
            string value = GetString(field);
            if (value.Length != 10 || !value.StartsWith("&H"))
                throw new FormatException(string.Format(Resources.IsNotAValidColor, value));

            if (!uint.TryParse(value.Substring(2), NumberStyles.AllowHexSpecifier, null, out uint abgr))
                throw new FormatException(string.Format(Resources.IsNotAValidColor, value));

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
                throw new FormatException(string.Format(Resources.IsNotAValidTimestamp, value));

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

        public override string ToString()
        {
            return Type;
        }
    }
}
