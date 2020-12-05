using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    public class MoveAnimation : Animation
    {
        public MoveAnimation(DateTime startTime, PointF startPos, DateTime endTime, PointF endPos)
            : base(startTime, endTime, 1)
        {
            StartPosition = startPos;
            EndPosition = endPos;
        }

        public PointF StartPosition
        {
            get;
        }

        public PointF EndPosition
        {
            get;
        }

        public override bool AffectsPast => true;

        public override void Apply(AssLine line, AssSection section, float t)
        {
            float x = Interpolate(StartPosition.X, EndPosition.X, t);
            float y = Interpolate(StartPosition.Y, EndPosition.Y, t);
            line.Position = new PointF(x, y);
        }

        public override object Clone()
        {
            return new MoveAnimation(StartTime, StartPosition, EndTime, EndPosition);
        }
    }
}
