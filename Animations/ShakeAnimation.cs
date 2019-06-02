using System;
using System.Drawing;
using Arc.YTSubConverter.Formats;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ShakeAnimation : Animation
    {
        private readonly Random _random;

        public ShakeAnimation(DateTime startTime, DateTime endTime, PointF center, SizeF radius)
            : base(startTime, endTime)
        {
            Center = center;
            Radius = radius;
            _random = new Random((int)(startTime - SubtitleDocument.TimeBase).TotalMilliseconds);
        }

        public PointF Center
        {
            get;
        }

        public SizeF Radius
        {
            get;
        }

        public override bool AffectsPast => false;

        public override void Apply(AssLine line, AssSection section, float t)
        {
            if (t > 0 && t < 1)
            {
                line.Position = new PointF(
                    Center.X + Radius.Width * ((float)_random.NextDouble() * 2 - 1.0f),
                    Center.Y + Radius.Height * ((float)_random.NextDouble() * 2 - 1.0f)
                );
            }
            else
            {
                line.Position = Center;
            }
        }

        public override object Clone()
        {
            return new ShakeAnimation(StartTime, EndTime, Center, Radius);
        }
    }
}
