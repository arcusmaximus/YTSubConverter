using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssForeAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "1a";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            int alpha = !string.IsNullOrEmpty(arg) ? 255 - (ParseHex(arg) & 255) : context.Style.PrimaryColor.A;
            context.Section.ForeColor = ColorUtil.ChangeColorAlpha(context.Section.ForeColor, alpha);
            context.Section.Animations.RemoveAll(a => a is ForeColorAnimation);
        }
    }
}
