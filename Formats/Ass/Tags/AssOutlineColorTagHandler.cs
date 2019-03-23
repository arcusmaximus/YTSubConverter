using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssOutlineColorTagHandler : AssTagHandlerBase
    {
        public override string Tag => "3c";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = ParseColor(arg, context.Section.BackColor.A);
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColors[ShadowType.Glow] = ParseColor(arg, context.Section.ShadowColors[ShadowType.Glow].A);
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }
    }
}
