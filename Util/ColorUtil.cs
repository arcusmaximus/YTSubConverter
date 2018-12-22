using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

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
    }
}
