using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Formats.Ass.KaraokeTypes;
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

        public IKaraokeType KaraokeType
        {
            get;
            set;
        }

        public void NormalizeAlpha()
        {
            if (Alpha == 255)
                return;

            float factor = Alpha / 255.0f;
            foreach (Section section in Sections)
            {
                section.ForeColor = MultiplyColorAlpha(section.ForeColor, factor);
                section.BackColor = MultiplyColorAlpha(section.BackColor, factor);
                foreach (KeyValuePair<ShadowType, Color> shadowColor in section.ShadowColors.ToList())
                {
                    section.ShadowColors[shadowColor.Key] = MultiplyColorAlpha(shadowColor.Value, factor);
                }
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

            AssLine assLine = (AssLine)line;
            Alpha = assLine.Alpha;
            Animations.Clear();
            Animations.AddRange(assLine.Animations.Select(a => (Animation)a.Clone()));
            KaraokeType = assLine.KaraokeType;
        }
    }
}
