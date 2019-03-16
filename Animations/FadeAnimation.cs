using System;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class FadeAnimation : Animation
    {
        public FadeAnimation(DateTime startTime, int startAlpha, DateTime endTime, int endAlpha)
            : base(startTime, endTime)
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
            int alpha = Interpolate(StartAlpha, EndAlpha, t);
            switch (line)
            {
                case AssLine assLine:
                    assLine.Alpha = alpha;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
