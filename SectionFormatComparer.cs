using System.Collections.Generic;
using System.Drawing;

namespace Arc.YTSubConverter
{
    internal struct SectionFormatComparer : IEqualityComparer<Section>
    {
        public bool Equals(Section x, Section y)
        {
            if (x.Bold != y.Bold ||
                x.Italic != y.Italic ||
                x.Underline != y.Underline ||
                x.Font != y.Font ||
                x.Scale != y.Scale ||
                x.Offset != y.Offset ||
                x.ForeColor != y.ForeColor ||
                x.BackColor != y.BackColor ||
                x.RubyPart != y.RubyPart ||
                x.ShadowColors.Count != y.ShadowColors.Count)
            {
                return false;
            }

            foreach (KeyValuePair<ShadowType, Color> xShadowColor in x.ShadowColors)
            {
                if (!y.ShadowColors.TryGetValue(xShadowColor.Key, out Color yShadowColor) || xShadowColor.Value != yShadowColor)
                    return false;
            }

            return true;
        }

        public int GetHashCode(Section section)
        {
            return section.Bold.GetHashCode() ^
                   section.Italic.GetHashCode() ^
                   section.Underline.GetHashCode() ^
                   (section.Font?.GetHashCode() ?? 0) ^
                   section.Scale.GetHashCode() ^
                   section.Offset.GetHashCode() ^
                   section.ForeColor.GetHashCode() ^
                   section.BackColor.GetHashCode() ^
                   section.RubyPart.GetHashCode();
        }
    }
}
