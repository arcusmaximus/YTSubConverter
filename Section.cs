using System;
using System.Drawing;

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

        public Color ShadowColor
        {
            get;
            set;
        }

        public ShadowType ShadowType
        {
            get;
            set;
        }

        public TimeSpan TimeOffset
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
            Bold = section.Bold;
            Italic = section.Italic;
            Underline = section.Underline;
            ForeColor = section.ForeColor;
            BackColor = section.BackColor;
            ShadowColor = section.ShadowColor;
            ShadowType = section.ShadowType;
            TimeOffset = section.TimeOffset;
        }
    }
}
