using System;

namespace Arc.YTSubConverter.Ass
{
    internal partial class AssDocument
    {
        private class ExtendedSection : Section
        {
            public ExtendedSection(string text)
                : base(text)
            {
            }

            public TimeSpan Duration
            {
                get;
                set;
            }

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
                Duration = extendedSection.Duration;
            }
        }
    }
}
