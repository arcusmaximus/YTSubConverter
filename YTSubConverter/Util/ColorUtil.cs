using System;
using System.Drawing;
using System.Globalization;

namespace Arc.YTSubConverter.Util
{
    public static class ColorUtil
    {
        public static Color ChangeColorAlpha(Color color, int alpha)
        {
            return Color.FromArgb((alpha << 24) | (color.ToArgb() & 0xFFFFFF));
        }

        public static bool IsDark(Color color)
        {
            return Math.Max(Math.Max(color.R, color.G), color.B) < 128;
        }

        public static Color Brighten(Color color)
        {
            int r = Math.Max(color.R, (byte)1);
            int g = Math.Max(color.G, (byte)1);
            int b = Math.Max(color.B, (byte)1);

            int highestComponent = Math.Max(Math.Max(r, g), b);
            float factor = 255f / highestComponent;
            return Color.FromArgb(
                color.A,
                (byte)Math.Round(r * factor),
                (byte)Math.Round(g * factor),
                (byte)Math.Round(b * factor)
            );
        }

        public static Color FromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return Color.Empty;

            if (html.Length != 7 || !html.StartsWith("#"))
                throw new FormatException();

            if (!int.TryParse(html.Substring(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int rgb))
                throw new FormatException();

            return Color.FromArgb((0xFF << 24) | rgb);
        }

        public static string ToHtml(Color color)
        {
            return $"#{color.ToArgb() & 0xFFFFFF:X06}";
        }
    }
}
