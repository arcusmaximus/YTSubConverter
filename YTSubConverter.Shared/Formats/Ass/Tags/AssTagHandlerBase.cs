using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal abstract class AssTagHandlerBase
    {
        public abstract string Tag
        {
            get;
        }

        public abstract bool AffectsWholeLine
        {
            get;
        }

        public abstract void Handle(AssTagContext context, string arg);

        protected static bool TryParseInt(string arg, out int value)
        {
            arg = arg.Replace("(", "")
                     .Replace(")", "")
                     .Replace(" ", "");
            return int.TryParse(arg, out value);
        }

        protected static int ParseInt(string arg)
        {
            TryParseInt(arg, out int value);
            return value;
        }

        protected static bool TryParseFloat(string arg, out float value)
        {
            arg = arg.Replace("(", "")
                     .Replace(")", "")
                     .Replace(" ", "");
            return float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        protected static int ParseHex(string arg)
        {
            arg = arg.Replace("&", "")
                     .Replace("H", "")
                     .Replace("(", "")
                     .Replace(")", "");
            int.TryParse(arg, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out int value);
            return value;
        }

        protected static List<float> ParseFloatList(string arg)
        {
            List<string> items = ParseStringList(arg);
            if (items == null)
                return null;

            List<float> list = new List<float>();
            foreach (string item in items)
            {
                float.TryParse(item.Replace(" ", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
                list.Add(value);
            }
            return list;
        }

        protected static List<string> ParseStringList(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
                return new List<string>();

            arg = arg.Trim();
            if (!arg.StartsWith("("))
                return null;

            return arg.Replace("(", "")
                      .Replace(")", "")
                      .Split(',')
                      .Select(i => i.Trim())
                      .ToList();
        }

        protected static Color ParseColor(string arg, int alpha)
        {
            int bgr = ParseHex(arg);
            byte r = (byte)bgr;
            byte g = (byte)(bgr >> 8);
            byte b = (byte)(bgr >> 16);
            return Color.FromArgb(alpha, r, g, b);
        }
    }
}
