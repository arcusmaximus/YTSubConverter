using System;
using System.Drawing;
using Arc.YTSubConverter.Shared.Formats;
using Arc.YTSubConverter.Shared.Formats.Ass;

namespace Arc.YTSubConverter.Shared.Animations
{
    public class ShakeAnimation : Animation
    {
        private readonly Random _random;

        public ShakeAnimation(DateTime startTime, DateTime endTime, SizeF radius)
            : base(startTime, endTime, 1)
        {
            Radius = radius;
            _random = new Random((int)(startTime - SubtitleDocument.TimeBase).TotalMilliseconds);
        }

        public SizeF Radius
        {
            get;
        }

        public override bool AffectsPast => false;

        public override void Apply(AssLine line, AssSection section, float t)
        {
            if (t <= 0 || t >= 1 || line.Position == null)
                return;

            line.Position = new PointF(
                line.Position.Value.X + Radius.Width * ((float)_random.NextDouble() * 2 - 1.0f),
                line.Position.Value.Y + Radius.Height * ((float)_random.NextDouble() * 2 - 1.0f)
            );
        }

        public override object Clone()
        {
            return new ShakeAnimation(StartTime, EndTime, Radius);
        }
    }
}
