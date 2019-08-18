using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssOutlineAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "3a";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            int alpha = !string.IsNullOrEmpty(arg) ? 255 - ParseHex(arg) : context.Style.OutlineColor.A;

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = ColorUtil.ChangeColorAlpha(context.Section.BackColor, alpha);
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColors[ShadowType.Glow] = ColorUtil.ChangeColorAlpha(context.Section.ShadowColors[ShadowType.Glow], alpha);
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }
    }
}
