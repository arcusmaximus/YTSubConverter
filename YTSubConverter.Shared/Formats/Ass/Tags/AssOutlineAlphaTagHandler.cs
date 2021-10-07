using YTSubConverter.Shared.Animations;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssOutlineAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "3a";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            int alpha = !string.IsNullOrEmpty(arg) ? 255 - (ParseHex(arg) & 255) : context.Style.OutlineColor.A;

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = ColorUtil.ChangeAlpha(context.Section.BackColor, alpha);
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColors[ShadowType.Glow] = ColorUtil.ChangeAlpha(context.Section.ShadowColors[ShadowType.Glow], alpha);
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }
    }
}
