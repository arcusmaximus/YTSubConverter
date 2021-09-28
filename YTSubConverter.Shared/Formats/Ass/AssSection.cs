using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Shared.Animations;

namespace Arc.YTSubConverter.Shared.Formats.Ass
{
    public class AssSection : Section
    {
        public AssSection()
        {
        }

        public AssSection(string text)
            : base(text)
        {
        }

        public AssSection(Section section)
            : base(section)
        {
        }

        public Color SecondaryColor
        {
            get;
            set;
        }

        public float Blur
        {
            get;
            set;
        }

        public Color CurrentWordForeColor
        {
            get;
            set;
        }

        public Color CurrentWordOutlineColor
        {
            get;
            set;
        }

        public Color CurrentWordShadowColor
        {
            get;
            set;
        }

        public TimeSpan Duration
        {
            get;
            set;
        }

        public List<Animation> Animations { get; } = new List<Animation>();

        public override object Clone()
        {
            return new AssSection(this);
        }

        protected override void Assign(Section section)
        {
            base.Assign(section);
            if (!(section is AssSection assSection))
                return;

            SecondaryColor = assSection.SecondaryColor;
            Blur = assSection.Blur;
            CurrentWordForeColor = assSection.CurrentWordForeColor;
            CurrentWordOutlineColor = assSection.CurrentWordOutlineColor;
            CurrentWordShadowColor = assSection.CurrentWordShadowColor;
            Duration = assSection.Duration;
            Animations.Clear();
            Animations.AddRange(assSection.Animations.Select(a => (Animation)a.Clone()));
        }
    }
}
