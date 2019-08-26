using System.Linq;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "alpha";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            int alpha = 255 - ParseHex(arg);
            context.Section.ForeColor = ColorUtil.ChangeColorAlpha(context.Section.ForeColor, alpha);
            context.Section.SecondaryColor = ColorUtil.ChangeColorAlpha(context.Section.SecondaryColor, alpha);
            context.Section.BackColor = ColorUtil.ChangeColorAlpha(context.Section.BackColor, alpha);
            foreach (ShadowType shadowType in context.Section.ShadowColors.Keys.ToList())
            {
                context.Section.ShadowColors[shadowType] = ColorUtil.ChangeColorAlpha(context.Section.ShadowColors[shadowType], alpha);
            }

            context.Section.Animations.RemoveAll(a => a is ColorAnimation);
        }
    }
}
