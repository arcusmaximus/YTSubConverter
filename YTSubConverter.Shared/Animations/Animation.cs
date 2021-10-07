using System;
using System.Drawing;
using YTSubConverter.Shared.Formats.Ass;

namespace YTSubConverter.Shared.Animations
{
    public abstract class Animation : ICloneable
    {
        protected Animation(DateTime startTime, DateTime endTime, float acceleration)
        {
            StartTime = startTime;
            EndTime = endTime;
            Acceleration = acceleration;
        }

        public DateTime StartTime
        {
            get;
        }

        public DateTime EndTime
        {
            get;
        }

        public float Acceleration
        {
            get;
        }

        public abstract bool AffectsPast
        {
            get;
        }

        public virtual bool AffectsText => false;

        public abstract void Apply(AssLine line, AssSection section, float t);

        protected int Interpolate(int from, int to, float t)
        {
            return from + (int)((to - from) * Math.Pow(t, Acceleration));
        }

        protected float Interpolate(float from, float to, float t)
        {
            return from + (to - from) * (float)Math.Pow(t, Acceleration);
        }

        protected Color Interpolate(Color from, Color to, float t)
        {
            return Color.FromArgb(
                Interpolate(from.A, to.A, t),
                Interpolate(from.R, to.R, t),
                Interpolate(from.G, to.G, t),
                Interpolate(from.B, to.B, t)
            );
        }

        public abstract object Clone();
    }
}
