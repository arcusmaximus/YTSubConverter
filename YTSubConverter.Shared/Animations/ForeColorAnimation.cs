using System;
using System.Drawing;
using YTSubConverter.Shared.Formats.Ass;

namespace YTSubConverter.Shared.Animations
{
    public class ForeColorAnimation : ColorAnimation
    {
        public ForeColorAnimation(DateTime startTime, Color startColor, DateTime endTime, Color endColor, float acceleration)
            : base(startTime, startColor, endTime, endColor, acceleration)
        {
        }

        public override void Apply(AssLine line, AssSection section, float t)
        {
            section.ForeColor = GetColor(t);
        }

        public override object Clone()
        {
            return new ForeColorAnimation(StartTime, StartColor, EndTime, EndColor, Acceleration);
        }
    }
}
