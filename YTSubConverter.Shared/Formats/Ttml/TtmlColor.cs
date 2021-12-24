using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public static class TtmlColor
    {
        public static bool TryParse(string text, out Color color)
        {
            if (string.IsNullOrEmpty(text))
            {
                color = Color.Empty;
                return false;
            }

            if (text.StartsWith("#"))
                return TryParseHash(text, out color);

            if (text.StartsWith("rgb"))
                return TryParseRgba(text, out color);

            if (text == "transparent")
            {
                color = Color.FromArgb(0, 0, 0, 0);
                return true;
            }

            if (!Regex.IsMatch(text, @"^[a-z]+$"))
                return false;

            color = Color.FromName(text);
            if (color != Color.FromName("invalid"))
                return true;

            color = Color.Empty;
            return false;
        }

        private static bool TryParseHash(string text, out Color color)
        {
            if (text.Length != 1 + 6 && text.Length != 1 + 8)
            {
                color = Color.Empty;
                return false;
            }

            int r, g, b, a;
            if (!int.TryParse(text.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r) ||
                !int.TryParse(text.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g) ||
                !int.TryParse(text.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
            {
                return false;
            }

            if (text.Length == 1 + 8)
            {
                if (!int.TryParse(text.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
                    return false;
            }
            else
            {
                a = 255;
            }

            color = Color.FromArgb(a, r, g, b);
            return true;
        }

        private static bool TryParseRgba(string text, out Color color)
        {
            color = Color.Empty;

            Match match = Regex.Match(text, @"^(rgba?)\(\s*(?:(\d+)\s*,?\s*)+\)$");
            if (!match.Success)
                return false;

            if (match.Groups[1].Value == "rgb" && match.Groups[2].Captures.Count != 3)
                return false;

            if (match.Groups[1].Value == "rgba" && match.Groups[2].Captures.Count != 4)
                return false;

            int r = int.Parse(match.Groups[2].Captures[0].Value);
            int g = int.Parse(match.Groups[2].Captures[1].Value);
            int b = int.Parse(match.Groups[2].Captures[2].Value);
            int a;
            if (match.Groups[1].Value == "rgba")
                a = int.Parse(match.Groups[2].Captures[3].Value);
            else
                a = 255;

            try
            {
                color = Color.FromArgb(a, r, g, b);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Color Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, out Color color))
                throw new FormatException();

            return color;
        }

        public static string ToString(Color color)
        {
            if (color.A == 255)
                return $"#{color.R:X02}{color.G:X02}{color.B:X02}";

            return $"#{color.R:X02}{color.G:X02}{color.B:X02}{color.A:X02}";
        }
    }
}
