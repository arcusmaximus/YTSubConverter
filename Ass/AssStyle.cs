using System;
using System.Drawing;

namespace Arc.YTSubConverter.Ass
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
            AnchorPoint = GetAnchorPointFromAlignment(item.GetInt("Alignment"));
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

        private static AnchorPoint GetAnchorPointFromAlignment(int alignment)
        {
            switch (alignment)
            {
                case 1:
                    return AnchorPoint.BottomLeft;

                case 2:
                    return AnchorPoint.BottomCenter;

                case 3:
                    return AnchorPoint.BottomRight;

                case 4:
                    return AnchorPoint.MiddleLeft;

                case 5:
                    return AnchorPoint.Center;

                case 6:
                    return AnchorPoint.MiddleRight;

                case 7:
                    return AnchorPoint.TopLeft;

                case 8:
                    return AnchorPoint.TopCenter;

                case 9:
                    return AnchorPoint.TopRight;

                default:
                    throw new ArgumentException($"{alignment} is not a valid alignment");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
