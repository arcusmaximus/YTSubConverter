using System;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssKaraokeTypeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytkt";

        public override void Handle(AssTagContext context, string arg)
        {
            if (Enum.TryParse(arg, out KaraokeType type))
                context.Line.KaraokeType = type;
        }
    }
}
