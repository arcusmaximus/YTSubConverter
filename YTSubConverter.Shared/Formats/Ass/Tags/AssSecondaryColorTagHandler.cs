using Arc.YTSubConverter.Shared.Animations;

namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssSecondaryColorTagHandler : AssTagHandlerBase
    {
        public override string Tag => "2c";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.SecondaryColor = !string.IsNullOrEmpty(arg) ? ParseColor(arg, context.Section.SecondaryColor.A) : context.Style.SecondaryColor;
            context.Section.Animations.RemoveAll(a => a is SecondaryColorAnimation);
        }
    }
}
