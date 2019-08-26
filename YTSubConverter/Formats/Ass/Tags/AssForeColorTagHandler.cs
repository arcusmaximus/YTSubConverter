using Arc.YTSubConverter.Animations;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssForeColorTagHandler : AssTagHandlerBase
    {
        public AssForeColorTagHandler(string tag)
        {
            Tag = tag;
        }

        public override string Tag
        {
            get;
        }

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.ForeColor = !string.IsNullOrEmpty(arg) ? ParseColor(arg, context.Section.ForeColor.A) : context.Style.PrimaryColor;
            context.Section.Animations.RemoveAll(a => a is ForeColorAnimation);
        }
    }
}
