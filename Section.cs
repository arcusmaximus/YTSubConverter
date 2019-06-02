using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter
{
    internal class Section : ICloneable
    {
        public Section(string text)
        {
            Text = text;
        }

        public string Text
        {
            get;
            set;
        }

        public string Font
        {
            get;
            set;
        }

        public float Scale
        {
            get;
            set;
        } = 1;

        public OffsetType Offset
        {
            get;
            set;
        }

        public bool Bold
        {
            get;
            set;
        }

        public bool Italic
        {
            get;
            set;
        }

        public bool Underline
        {
            get;
            set;
        }

        public Color ForeColor
        {
            get;
            set;
        }

        public Color BackColor
        {
            get;
            set;
        }

        public Dictionary<ShadowType, Color> ShadowColors { get; } = new Dictionary<ShadowType, Color>();

        public RubyPart RubyPart
        {
            get;
            set;
        }

        public RubyPosition RubyPosition
        {
            get;
            set;
        }

        public bool Packed
        {
            get;
            set;
        }

        public TimeSpan StartOffset
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Text;
        }

        public virtual object Clone()
        {
            Section newSection = new Section(Text);
            newSection.Assign(this);
            return newSection;
        }

        protected virtual void Assign(Section section)
        {
            Text = section.Text;
            Font = section.Font;
            Scale = section.Scale;
            Offset = section.Offset;
            Bold = section.Bold;
            Italic = section.Italic;
            Underline = section.Underline;
            ForeColor = section.ForeColor;
            BackColor = section.BackColor;
            ShadowColors.Clear();
            ShadowColors.AddRange(section.ShadowColors);
            RubyPart = section.RubyPart;
            RubyPosition = section.RubyPosition;
            Packed = section.Packed;
            StartOffset = section.StartOffset;
        }
    }
}
