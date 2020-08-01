using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ForeColorAnimation : ColorAnimation
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
