using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal abstract class Animation
    {
        protected Animation(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public DateTime StartTime
        {
            get;
        }

        public DateTime EndTime
        {
            get;
        }

        public abstract bool AffectsPast
        {
            get;
        }

        public virtual bool AffectsText => false;

        public abstract void Apply(AssLine line, AssSection section, float t);

        protected static int Interpolate(int from, int to, float t)
        {
            return from + (int)((to - from) * t);
        }

        protected static float Interpolate(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        protected static Color Interpolate(Color from, Color to, float t)
        {
            return Color.FromArgb(
                Interpolate(from.A, to.A, t),
                Interpolate(from.R, to.R, t),
                Interpolate(from.G, to.G, t),
                Interpolate(from.B, to.B, t)
            );
        }
    }
}
