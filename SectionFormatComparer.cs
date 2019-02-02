using System.Collections.Generic;

namespace Arc.YTSubConverter
{
    internal struct SectionFormatComparer : IEqualityComparer<Section>
    {
        public bool Equals(Section x, Section y)
        {
            return x.Bold == y.Bold &&
                   x.Italic == y.Italic &&
                   x.Underline == y.Underline &&
                   x.Font == y.Font &&
                   x.ForeColor == y.ForeColor &&
                   x.BackColor == y.BackColor &&
                   x.ShadowColor == y.ShadowColor &&
                   x.ShadowTypes == y.ShadowTypes;
        }

        public int GetHashCode(Section section)
        {
            return section.Bold.GetHashCode() ^
                   section.Italic.GetHashCode() ^
                   section.Underline.GetHashCode() ^
                   (section.Font?.GetHashCode() ?? 0) ^
                   section.ForeColor.GetHashCode() ^
                   section.BackColor.GetHashCode() ^
                   section.ShadowColor.GetHashCode() ^
                   section.ShadowTypes.GetHashCode();
        }
    }
}
