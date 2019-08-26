using System;
using System.Collections.Generic;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssComplexFadeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fade";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            List<float> args = ParseFloatList(arg);
            if (args == null || args.Count != 7)
                return;

            AssLine line = context.Line;

            int initialAlpha = 255 - (int)args[0];
            int midAlpha = 255 - (int)args[1];
            int finalAlpha = 255 - (int)args[2];

            DateTime fadeInStartTime = line.Start.AddMilliseconds(args[3]);
            DateTime fadeInEndTime = line.Start.AddMilliseconds(args[4]);
            DateTime fadeOutStartTime = line.Start.AddMilliseconds(args[5]);
            DateTime fadeOutEndTime = line.Start.AddMilliseconds(args[6]);

            if (fadeInEndTime > fadeInStartTime)
                line.Animations.Add(new FadeAnimation(fadeInStartTime, initialAlpha, fadeInEndTime, midAlpha));

            if (fadeOutEndTime > fadeOutStartTime)
                line.Animations.Add(new FadeAnimation(fadeOutStartTime, midAlpha, fadeOutEndTime, finalAlpha));
        }
    }
}
