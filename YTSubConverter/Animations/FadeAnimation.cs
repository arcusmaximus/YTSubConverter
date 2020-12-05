using System;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    public class FadeAnimation : Animation
    {
        public FadeAnimation(DateTime startTime, int startAlpha, DateTime endTime, int endAlpha)
            : base(startTime, endTime, 1)
        {
            StartAlpha = startAlpha;
            EndAlpha = endAlpha;
        }

        public int StartAlpha
        {
            get;
        }

        public int EndAlpha
        {
            get;
        }

        public override bool AffectsPast => true;

        public override void Apply(AssLine line, AssSection section, float t)
        {
            line.Alpha = Interpolate(StartAlpha, EndAlpha, t);
        }

        public override object Clone()
        {
            return new FadeAnimation(StartTime, StartAlpha, EndTime, EndAlpha);
        }
    }
}
