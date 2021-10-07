using System;
using System.Collections.Generic;
using System.Drawing;

namespace YTSubConverter.Shared
{
    internal class SectionFormatComparer : IEqualityComparer<Section>
    {
        public bool Equals(Section x, Section y)
        {
            if (x.Bold != y.Bold ||
                x.Italic != y.Italic ||
                x.Underline != y.Underline ||
                NormalizeFont(x.Font) != NormalizeFont(y.Font) ||
                Math.Abs(NormalizeScale(x.Scale) - NormalizeScale(y.Scale)) > 0.001 ||
                x.Offset != y.Offset ||
                x.ForeColor.ToArgb() != y.ForeColor.ToArgb() ||
                x.BackColor.ToArgb() != y.BackColor.ToArgb() ||
                x.RubyPart != y.RubyPart ||
                x.Packed != y.Packed ||
                x.ShadowColors.Count != y.ShadowColors.Count)
            {
                return false;
            }

            foreach (KeyValuePair<ShadowType, Color> xShadowColor in x.ShadowColors)
            {
                if (!y.ShadowColors.TryGetValue(xShadowColor.Key, out Color yShadowColor) || xShadowColor.Value.ToArgb() != yShadowColor.ToArgb())
                    return false;
            }

            return true;
        }

        public int GetHashCode(Section section)
        {
            return section.Bold.GetHashCode() ^
                   section.Italic.GetHashCode() ^
                   section.Underline.GetHashCode() ^
                   (NormalizeFont(section.Font)?.GetHashCode() ?? 0) ^
                   section.Offset.GetHashCode() ^
                   section.ForeColor.ToArgb() ^
                   section.BackColor.ToArgb() ^
                   section.RubyPart.GetHashCode() ^
                   section.Packed.GetHashCode();
        }

        protected virtual string NormalizeFont(string font)
        {
            return font;
        }

        protected virtual float NormalizeScale(float scale)
        {
            return scale;
        }
    }
}
