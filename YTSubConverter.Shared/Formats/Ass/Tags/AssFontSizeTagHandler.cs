using YTSubConverter.Shared.Animations;

namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssFontSizeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fs";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!TryParseFloat(arg, out float lineHeight))
                lineHeight = context.Style.LineHeight;

            context.Section.Scale = lineHeight / context.Document.DefaultStyle.LineHeight;
            context.Section.Animations.RemoveAll(a => a is ScaleAnimation);
        }
    }
}
