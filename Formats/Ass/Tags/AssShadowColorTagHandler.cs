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

            context.Section.ShadowColor = ParseColor(arg, context.Section.ShadowColor.A);
            context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
        }
    }
}
