using System;
using System.Collections.Generic;

namespace Arc.YTSubConverter.Util
{
    /// <summary>
    /// Converts between Aegisub line heights and real font sizes, as these
    /// are indeed different depending on the font. (For example, YouTube uses
    /// a default size of 32px for all fonts at 720p, but to get the same size
    /// in Aegisub, you need to specify 38px for Roboto, 45px for Comic Sans etc.)
    /// </summary>
    public static class FontSizeMapper
    {
        private static readonly Dictionary<string, float> LineHeightToFontSizeFactors =
            new Dictionary<string, float>
            {
                { "carrois gothic sc", 32f / 38f },
                { "comic sans ms",     32f / 45f },
                { "courier new",       32f / 36f },
                { "lucida console",    32f / 32f },
                { "monotype corsiva",  32f / 35f },
                { "roboto",            32f / 38f },
                { "times new roman",   32f / 35f }
            };

        public static float LineHeightToFontSize(string fontName, float lineHeight)
        {
            return lineHeight * GetSizeFactor(fontName);
        }

        public static float FontSizeToLineHeight(string fontName, float fontSize)
        {
            return fontSize / GetSizeFactor(fontName);
        }

        private static float GetSizeFactor(string fontName)
        {
            if (!LineHeightToFontSizeFactors.TryGetValue(fontName.ToLower(), out float factor))
                throw new NotSupportedException($"No size factor defined for font {fontName}");

            return factor;
        }
    }
}
