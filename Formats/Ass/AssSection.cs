using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssSection : Section
    {
        public AssSection(string text)
            : base(text)
        {
        }

        public Color SecondaryColor
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
            AssSection newSection = new AssSection(Text);
            newSection.Assign(this);
            return newSection;
        }

        protected override void Assign(Section section)
        {
            base.Assign(section);

            AssSection assSection = (AssSection)section;
            SecondaryColor = assSection.SecondaryColor;
            CurrentWordForeColor = assSection.CurrentWordForeColor;
            CurrentWordOutlineColor = assSection.CurrentWordOutlineColor;
            CurrentWordShadowColor = assSection.CurrentWordShadowColor;
            Duration = assSection.Duration;
            Animations.Clear();
            Animations.AddRange(assSection.Animations.Select(a => (Animation)a.Clone()));
        }
    }
}
