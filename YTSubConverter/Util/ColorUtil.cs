using System;
using System.Drawing;
using System.Globalization;

namespace Arc.YTSubConverter.Util
{
    internal static class ColorUtil
    {
        public static Color ChangeColorAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
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
                (byte)(r * factor),
                (byte)(g * factor),
                (byte)(b * factor)
            );
        }

        public static Color FromHtml(string html)
        {
            if (html == null || html.Length != 7 || !html.StartsWith("#"))
                return Color.Empty;

            if (!int.TryParse(html.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int r) ||
                !int.TryParse(html.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int g) ||
                !int.TryParse(html.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int b))
            {
                return Color.Empty;
            }

            return Color.FromArgb(255, r, g, b);
        }

        public static string ToHtml(Color color)
        {
            if (color.IsEmpty)
                return string.Empty;

            return $"#{color.R:X02}{color.G:X02}{color.B:X02}";
        }
    }
}
