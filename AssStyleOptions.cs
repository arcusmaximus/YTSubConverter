﻿using System.Drawing;
using System.Xml.Serialization;
using Arc.YTSubConverter.Ass;
using Arc.YTSubConverter.Util;

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
                ShadowType = ShadowType.Glow;
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

        public bool IsKaraoke
        {
            get;
            set;
        }

        [XmlIgnore]
        public Color CurrentWordTextColor
        {
            get;
            set;
        }

        [XmlElement("CurrentWordTextColor")]
        public string CurrentWordTextColorHtml
        {
            get { return ColorUtil.ToHtml(CurrentWordTextColor); }
            set { CurrentWordTextColor = ColorUtil.FromHtml(value); }
        }

        [XmlIgnore]
        public Color CurrentWordShadowColor
        {
            get;
            set;
        }

        [XmlElement("CurrentWordShadowColor")]
        public string CurrentWordShadowColorHtml
        {
            get { return ColorUtil.ToHtml(CurrentWordShadowColor); }
            set { CurrentWordShadowColor = ColorUtil.FromHtml(value); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}