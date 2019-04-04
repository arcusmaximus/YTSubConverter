using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            foreach (KeyValuePair<ShadowType, Color> shadowColor in context.Section.ShadowColors.ToList())
            {
                if (shadowColor.Key != ShadowType.Glow || !context.Style.HasOutlineBox)
                    context.Section.ShadowColors[shadowColor.Key] = ColorUtil.ChangeColorAlpha(shadowColor.Value, alpha);
            }
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
