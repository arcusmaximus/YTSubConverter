using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssShadowColorTagHandler : AssTagHandlerBase
    {
        public override string Tag => "4c";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            foreach (KeyValuePair<ShadowType, Color> shadowColor in context.Section.ShadowColors.ToList())
            {
                if (shadowColor.Key != ShadowType.Glow || !context.Style.HasOutlineBox)
                    context.Section.ShadowColors[shadowColor.Key] = ParseColor(arg, shadowColor.Value.A);
            }

            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
