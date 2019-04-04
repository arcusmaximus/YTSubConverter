using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using Arc.YTSubConverter.Formats.Ass;
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
                ShadowTypes.Add(ShadowType.Glow);

            if (style.HasShadow)
                ShadowTypes.Add(ShadowType.SoftShadow);
        }

        public string Name
        {
            get;
            set;
        }

        [XmlElement("ShadowType")]
        public XmlShadowTypes ShadowTypesBitmask
        {
            get
            {
                XmlShadowTypes shadowTypes = XmlShadowTypes.None;

                if (ShadowTypes.Contains(ShadowType.Glow))
                    shadowTypes |= XmlShadowTypes.Glow;

                if (ShadowTypes.Contains(ShadowType.Bevel))
                    shadowTypes |= XmlShadowTypes.Bevel;

                if (ShadowTypes.Contains(ShadowType.SoftShadow))
                    shadowTypes |= XmlShadowTypes.SoftShadow;

                if (ShadowTypes.Contains(ShadowType.HardShadow))
                    shadowTypes |= XmlShadowTypes.HardShadow;

                return shadowTypes;
            }
            set
            {
                ShadowTypes.Clear();
                if ((value & XmlShadowTypes.Glow) != 0)
                    ShadowTypes.Add(ShadowType.Glow);

                if ((value & XmlShadowTypes.Bevel) != 0)
                    ShadowTypes.Add(ShadowType.Bevel);

                if ((value & XmlShadowTypes.SoftShadow) != 0)
                    ShadowTypes.Add(ShadowType.SoftShadow);

                if ((value & XmlShadowTypes.HardShadow) != 0)
                    ShadowTypes.Add(ShadowType.HardShadow);
            }
        }

        [XmlIgnore]
        public List<ShadowType> ShadowTypes
        {
            get;
        } = new List<ShadowType>();

        public void SetShadowTypeEnabled(ShadowType type, bool enable)
        {
            if (enable)
            {
                if (!ShadowTypes.Contains(type))
                    ShadowTypes.Add(type);
            }
            else
            {
                ShadowTypes.Remove(type);
            }
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
        public Color CurrentWordOutlineColor
        {
            get;
            set;
        }

        [XmlElement("CurrentWordOutlineColor")]
        public string CurrentWordOutlineColorHtml
        {
            get { return ColorUtil.ToHtml(CurrentWordOutlineColor); }
            set { CurrentWordOutlineColor = ColorUtil.FromHtml(value); }
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

        [Flags]
        public enum XmlShadowTypes
        {
            None = 0,
            Glow = 1,
            HardShadow = 2,
            SoftShadow = 4,
            Bevel = 8
        }
    }
}
