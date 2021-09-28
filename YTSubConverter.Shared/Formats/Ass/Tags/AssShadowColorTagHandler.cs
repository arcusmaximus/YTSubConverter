using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Arc.YTSubConverter.Shared.Animations;

namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssShadowColorTagHandler : AssTagHandlerBase
    {
        public override string Tag => "4c";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasShadow)
                return;

            foreach (KeyValuePair<ShadowType, Color> shadowColor in context.Section.ShadowColors.ToList())
            {
                if (shadowColor.Key != ShadowType.Glow || !context.Style.HasOutline || context.Style.HasOutlineBox)
                    context.Section.ShadowColors[shadowColor.Key] = !string.IsNullOrEmpty(arg) ? ParseColor(arg, shadowColor.Value.A) : context.Style.ShadowColor;
            }

            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
