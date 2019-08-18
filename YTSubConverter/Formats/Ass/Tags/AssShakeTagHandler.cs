using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    /// <summary>
    /// Nonstandard tag: \ytshake, \ytshake(radius), \ytshake(radiusX, radiusY), \ytshake(radius, t1, t2), \ytshake(radiusX, radiusY, t1, t2)
    /// </summary>
    internal class AssShakeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytshake";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!TryParseArgs(context, arg, out SizeF radius, out DateTime startTime, out DateTime endTime))
                return;

            context.PostProcessors.Add(
                () =>
                {
                    PointF center = context.Line.Position ?? context.Document.GetDefaultPosition(context.Line.AnchorPoint);
                    context.Line.Animations.Add(new ShakeAnimation(startTime, endTime, center, radius));
                    return null;
                }
            );
        }

        private static bool TryParseArgs(AssTagContext context, string arg, out SizeF radius, out DateTime startTime, out DateTime endTime)
        {
            int defaultRadius = 20;
            radius = new SizeF(defaultRadius, defaultRadius);
            startTime = context.Line.Start;
            endTime = context.Line.End;

            if (string.IsNullOrWhiteSpace(arg))
                return true;

            List<float> args = ParseFloatList(arg);
            if (args == null)
                return false;

            switch (args.Count)
            {
                case 0:
                    return true;

                case 1:
                    radius = new SizeF(args[0], args[0]);
                    return true;

                case 2:
                    radius = new SizeF(args[0], args[1]);
                    return true;

                case 3:
                    radius = new SizeF(args[0], args[0]);
                    startTime = context.Line.Start.AddMilliseconds(args[1]);
                    endTime = context.Line.Start.AddMilliseconds(args[2]);
                    return true;

                case 4:
                    radius = new SizeF(args[0], args[1]);
                    startTime = context.Line.Start.AddMilliseconds(args[2]);
                    endTime = context.Line.Start.AddMilliseconds(args[3]);
                    return true;

                default:
                    return false;
            }
        }
    }
}
