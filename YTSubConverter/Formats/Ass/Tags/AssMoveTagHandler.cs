using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssMoveTagHandler : AssTagHandlerBase
    {
        public override string Tag => "move";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            List<float> args = ParseFloatList(arg);
            if (args == null || args.Count < 4)
                return;

            AssLine line = context.Line;
            PointF startPos = new PointF(args[0], args[1]);
            PointF endPos = new PointF(args[2], args[3]);

            DateTime startTime = line.Start;
            DateTime endTime = line.End;
            if (args.Count >= 6)
            {
                startTime = line.Start.AddMilliseconds(args[4]);
                endTime = line.Start.AddMilliseconds(args[5]);
            }

            if (endTime > startTime)
                line.Animations.Add(new MoveAnimation(startTime, startPos, endTime, endPos));
        }
    }
}
