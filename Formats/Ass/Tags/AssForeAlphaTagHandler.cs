using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssForeAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "1a";

        public override void Handle(AssTagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.ForeColor = ColorUtil.ChangeColorAlpha(context.Section.ForeColor, alpha);
            context.Section.Animations.RemoveAll(a => a is ForeColorAnimation);
        }
    }
}
