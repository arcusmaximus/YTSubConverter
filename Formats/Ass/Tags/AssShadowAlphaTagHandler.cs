using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssShadowAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "4a";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            int alpha = 255 - ParseHex(arg);
            context.Section.ShadowColor = ColorUtil.ChangeColorAlpha(context.Section.ShadowColor, alpha);
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
