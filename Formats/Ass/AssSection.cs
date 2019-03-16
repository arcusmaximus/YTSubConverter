using System;
using System.Collections.Generic;
using System.Drawing;
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

        public Color CurrentWordTextColor
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

            AssSection extendedSection = (AssSection)section;
            SecondaryColor = extendedSection.SecondaryColor;
            CurrentWordTextColor = extendedSection.CurrentWordTextColor;
            CurrentWordShadowColor = extendedSection.CurrentWordShadowColor;
            Duration = extendedSection.Duration;
            Animations.Clear();
            Animations.AddRange(extendedSection.Animations);
        }
    }
}
