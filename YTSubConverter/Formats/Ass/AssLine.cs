using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Formats.Ass.KaraokeTypes;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    public class AssLine : Line
    {
        public AssLine(DateTime start, DateTime end)
            : base(start, end)
        {
        }

        public AssLine(Line line)
            : base(line)
        {
            if (line is AssLine || Sections.All(s => s.StartOffset == TimeSpan.Zero))
                return;

            for (int i = 0; i < Sections.Count - 1; i++)
            {
                ((AssSection)Sections[i]).Duration = Sections[i + 1].StartOffset - Sections[i].StartOffset;
            }

            AssSection lastSection = (AssSection)Sections.Last();
            lastSection.Duration = End - Start - lastSection.StartOffset;
        }

        public int Alpha { get; set; } = 255;

        public List<Animation> Animations { get; } = new List<Animation>();

        public IKaraokeType KaraokeType { get; set; } = SimpleKaraokeType.Instance;

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
            return new AssLine(this);
        }

        protected override void Assign(Line line)
        {
            base.Assign(line);
            if (!(line is AssLine assLine))
                return;

            Alpha = assLine.Alpha;
            Animations.Clear();
            Animations.AddRange(assLine.Animations.Select(a => (Animation)a.Clone()));
            KaraokeType = assLine.KaraokeType;
        }

        protected override Section CreateSection(Section section)
        {
            return new AssSection(section);
        }
    }
}
