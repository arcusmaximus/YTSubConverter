using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal partial class AssDocument
    {
        public class ExtendedSection : Section
        {
            public ExtendedSection(string text)
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
                ExtendedSection newSection = new ExtendedSection(Text);
                newSection.Assign(this);
                return newSection;
            }

            protected override void Assign(Section section)
            {
                base.Assign(section);

                ExtendedSection extendedSection = (ExtendedSection)section;
                SecondaryColor = extendedSection.SecondaryColor;
                CurrentWordTextColor = extendedSection.CurrentWordTextColor;
                CurrentWordShadowColor = extendedSection.CurrentWordShadowColor;
                Duration = extendedSection.Duration;
                Animations.Clear();
                Animations.AddRange(extendedSection.Animations);
            }
        }
    }
}
