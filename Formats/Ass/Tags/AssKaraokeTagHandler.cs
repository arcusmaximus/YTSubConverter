using System;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssKaraokeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "k";

        public override void Handle(AssTagContext context, string arg)
        {
            int centiSeconds = int.Parse(arg);
            context.Section.Duration = TimeSpan.FromMilliseconds(centiSeconds * 10);
        }
    }
}
