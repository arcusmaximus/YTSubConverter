using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal abstract class AssTagHandlerBase
    {
        public abstract string Tag
        {
            get;
        }

        public abstract void Handle(AssTagContext context, string arg);

        protected static int ParseHex(string arg)
        {
            if (!arg.StartsWith("&H") || !arg.EndsWith("&"))
                return 0;

            int.TryParse(arg.Substring(2, arg.Length - 3), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int value);
            return value;
        }

        protected static Color ParseColor(string arg, int alpha)
        {
            int bgr = ParseHex(arg);
            byte r = (byte)bgr;
            byte g = (byte)(bgr >> 8);
            byte b = (byte)(bgr >> 16);
            return Color.FromArgb(alpha, r, g, b);
        }

        protected static List<float> ParseNumberList(string arg)
        {
            Match match = Regex.Match(arg, @"^\s*\((?:\s*,?\s*([\d\.]+))+\s*\)\s*$");
            if (!match.Success)
                return null;

            List<float> list = new List<float>();
            foreach (Capture capture in match.Groups[1].Captures)
            {
                list.Add(float.Parse(capture.Value, CultureInfo.InvariantCulture));
            }
            return list;
        }

        protected static List<string> ParseStringList(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
                return new List<string>();

            Match match = Regex.Match(arg, @"^\s*\((?:\s*,?\s*([^,\(\)]+))+\)\s*$");
            if (!match.Success)
                return null;

            return match.Groups[1].Captures
                        .Cast<Capture>()
                        .Select(c => c.Value.Trim())
                        .ToList();
        }
    }
}
