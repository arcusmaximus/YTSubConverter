using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using YTSubConverter.Shared.Animations;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssShadowAlphaTagHandler : AssTagHandlerBase
    {
        public override string Tag => "4a";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            int alpha = !string.IsNullOrEmpty(arg) ? 255 - (ParseHex(arg) & 255) : context.Style.ShadowColor.A;
            foreach (KeyValuePair<ShadowType, Color> shadowColor in context.Section.ShadowColors.ToList())
            {
                if (shadowColor.Key != ShadowType.Glow || !context.Style.HasOutline || context.Style.HasOutlineBox)
                    context.Section.ShadowColors[shadowColor.Key] = ColorUtil.ChangeAlpha(shadowColor.Value, alpha);
            }
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
