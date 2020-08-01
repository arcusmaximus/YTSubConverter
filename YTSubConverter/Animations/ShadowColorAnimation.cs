using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ShadowColorAnimation : ColorAnimation
    {
        public ShadowColorAnimation(ShadowType shadowType, DateTime startTime, Color startColor, DateTime endTime, Color endColor, float acceleration)
            : base(startTime, startColor, endTime, endColor, acceleration)
        {
            ShadowType = shadowType;
        }

        public ShadowType ShadowType
        {
            get;
        }

        public override void Apply(AssLine line, AssSection section, float t)
        {
            section.ShadowColors[ShadowType] = GetColor(t);
        }

        public override object Clone()
        {
            return new ShadowColorAnimation(ShadowType, StartTime, StartColor, EndTime, EndColor, Acceleration);
        }
    }
}
