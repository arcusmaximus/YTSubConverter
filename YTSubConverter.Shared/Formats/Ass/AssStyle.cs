using System.Drawing;

namespace YTSubConverter.Shared.Formats.Ass
{
    public class AssStyle
    {
        internal AssStyle(AssDocumentItem item)
        {
            Name = item.GetString("Name");
            Font = item.GetString("Fontname");
            LineHeight = item.GetFloat("Fontsize");
            Bold = item.GetBool("Bold");
            Italic = item.GetBool("Italic");
            Underline = item.GetBool("Underline");
            PrimaryColor = item.GetColor("PrimaryColour");
            SecondaryColor = item.GetColor("SecondaryColour");
            OutlineColor = item.GetColor("OutlineColour");
            OutlineThickness = item.GetFloat("Outline");
            OutlineIsBox = item.GetInt("BorderStyle") == 3;
            ShadowColor = item.GetColor("BackColour");
            ShadowDistance = item.GetFloat("Shadow");
            AnchorPoint = AssDocument.GetAnchorPoint(item.GetInt("Alignment"));
        }

        public string Name
        {
            get;
        }

        public string Font
        {
            get;
        }

        public float LineHeight
        {
            get;
            set;
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

        public float ShadowDistance
        {
            get;
        }

        public bool HasShadow => ShadowDistance > 0;

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
