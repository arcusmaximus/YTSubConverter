using System.Linq;
using Arc.YTSubConverter.Shared.Animations;
using Arc.YTSubConverter.Shared.Util;

namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "alpha";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            int alpha = 255 - (ParseHex(arg) & 255);
            context.Section.ForeColor = ColorUtil.ChangeAlpha(context.Section.ForeColor, alpha);
            context.Section.SecondaryColor = ColorUtil.ChangeAlpha(context.Section.SecondaryColor, alpha);

            if (context.Style.HasOutlineBox)
                context.Section.BackColor = ColorUtil.ChangeAlpha(context.Section.BackColor, alpha);

            foreach (ShadowType shadowType in context.Section.ShadowColors.Keys.ToList())
            {
                context.Section.ShadowColors[shadowType] = ColorUtil.ChangeAlpha(context.Section.ShadowColors[shadowType], alpha);
            }

            context.Section.Animations.RemoveAll(a => a is ColorAnimation);
        }
    }
}
