using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssLine : Line
    {
        public AssLine(DateTime start, DateTime end)
            : base(start, end)
        {
        }

        public int Alpha
        {
            get;
            set;
        } = 255;

        public List<Animation> Animations { get; } = new List<Animation>();

        public void NormalizeAlpha()
        {
            if (Alpha == 255)
                return;

            float factor = Alpha / 255.0f;
            foreach (Section section in Sections)
            {
                section.ForeColor = MultiplyColorAlpha(section.ForeColor, factor);
                section.BackColor = MultiplyColorAlpha(section.BackColor, factor);
                section.ShadowColor = MultiplyColorAlpha(section.ShadowColor, factor);
            }
            Alpha = 255;
        }

        private static Color MultiplyColorAlpha(Color color, float factor)
        {
            if (color.IsEmpty)
                return color;

            return ColorUtil.ChangeColorAlpha(color, (int)(color.A * factor));
        }

        public override object Clone()
        {
            AssLine newLine = new AssLine(Start, End);
            newLine.Assign(this);
            return newLine;
        }

        protected override void Assign(Line line)
        {
            base.Assign(line);

            AssLine extendedLine = (AssLine)line;
            Alpha = extendedLine.Alpha;
            Animations.Clear();
            Animations.AddRange(extendedLine.Animations);
        }
    }
}
