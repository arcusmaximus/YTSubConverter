using System.Drawing;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssStyle
    {
        public AssStyle(AssItem item)
        {
            Name = item.GetString("Name");
            Font = item.GetString("Fontname");
            Bold = item.GetBool("Bold");
            Italic = item.GetBool("Italic");
            Underline = item.GetBool("Underline");
            PrimaryColor = item.GetColor("PrimaryColour");
            SecondaryColor = item.GetColor("SecondaryColour");
            OutlineColor = item.GetColor("OutlineColour");
            OutlineThickness = item.GetFloat("Outline");
            OutlineIsBox = item.GetInt("BorderStyle") == 3;
            ShadowColor = item.GetColor("BackColour");
            ShadowThickness = item.GetFloat("Shadow");
            AnchorPoint = AssDocument.GetAnchorPointFromAlignment(item.GetInt("Alignment"));
        }

        public string Name
        {
            get;
        }

        public string Font
        {
            get;
        }

        public bool Bold
        {
            get;
        }

        public bool Italic
        {
            get;
        }

        public bool Underline
        {
            get;
        }

        public Color PrimaryColor
        {
            get;
        }

        public Color SecondaryColor
        {
            get;
        }

        public Color OutlineColor
        {
            get;
        }

        public float OutlineThickness
        {
            get;
        }

        public bool OutlineIsBox
        {
            get;
        }

        public bool HasOutline => OutlineThickness > 0;

        public bool HasOutlineBox => HasOutline && OutlineIsBox;

        public Color ShadowColor
        {
            get;
        }

        public float ShadowThickness
        {
            get;
        }

        public bool HasShadow => ShadowThickness > 0;

        public AnchorPoint AnchorPoint
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
