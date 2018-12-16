using Arc.YTSubConverter.Ass;

namespace Arc.YTSubConverter
{
    public class AssStyleOptions
    {
        public AssStyleOptions()
        {
        }

        internal AssStyleOptions(AssStyle style)
        {
            Name = style.Name;
            if (style.HasOutline && !style.HasOutlineBox)
                ShadowType = ShadowType.Glow;
            else if (style.HasShadow)
                ShadowType = ShadowType.SoftShadow;
            else
                ShadowType = ShadowType.None;
        }

        public string Name
        {
            get;
            set;
        }

        public ShadowType ShadowType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
