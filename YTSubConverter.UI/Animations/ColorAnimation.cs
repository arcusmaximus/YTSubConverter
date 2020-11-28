using System;
using System.Drawing;

namespace Arc.YTSubConverter.Animations
{
    internal abstract class ColorAnimation : Animation
    {
        protected ColorAnimation(DateTime startTime, Color startColor, DateTime endTime, Color endColor, float acceleration)
            : base(startTime, endTime, acceleration)
        {
            StartColor = startColor;
            EndColor = endColor;
        }

        public Color StartColor
        {
            get;
            set;
        }

        public Color EndColor
        {
            get;
            set;
        }

        public override bool AffectsPast => false;

        protected Color GetColor(float t)
        {
            return Interpolate(StartColor, EndColor, t);
        }
    }
}
