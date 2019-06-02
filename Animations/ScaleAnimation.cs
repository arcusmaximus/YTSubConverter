using System;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ScaleAnimation : Animation
    {
        public ScaleAnimation(DateTime startTime, float startScale, DateTime endTime, float endScale)
            : base(startTime, endTime)
        {
            StartScale = startScale;
            EndScale = endScale;
        }

        public override bool AffectsPast => false;

        public float StartScale
        {
            get;
        }

        public float EndScale
        {
            get;
        }

        public override void Apply(AssLine line, AssSection section, float t)
        {
            section.Scale = Interpolate(StartScale, EndScale, t);
        }

        public override object Clone()
        {
            return new ScaleAnimation(StartTime, StartScale, EndTime, EndScale);
        }
    }
}
