using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssOutlineColorTagHandler : AssTagHandlerBase
    {
        public override string Tag => "3c";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!context.Style.HasOutline)
                return;

            if (context.Style.OutlineIsBox)
            {
                context.Section.BackColor = !string.IsNullOrEmpty(arg) ? ParseColor(arg, context.Section.BackColor.A) : context.Style.OutlineColor;
                context.Section.Animations.RemoveAll(a => a is BackColorAnimation);
            }
            else
            {
                context.Section.ShadowColors[ShadowType.Glow] = !string.IsNullOrEmpty(arg) ? ParseColor(arg, context.Section.ShadowColors[ShadowType.Glow].A) : context.Style.OutlineColor;
                context.Section.Animations.RemoveAll(a => a is ShadowColorAnimation);
            }
        }
    }
}
