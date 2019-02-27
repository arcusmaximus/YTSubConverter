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

        public override void Apply(AssDocument.ExtendedLine line, AssDocument.ExtendedSection section, float t)
        {
            int alpha = Interpolate(StartAlpha, EndAlpha, t);
            switch (line)
            {
                case AssDocument.ExtendedLine assLine:
                    assLine.Alpha = alpha;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
