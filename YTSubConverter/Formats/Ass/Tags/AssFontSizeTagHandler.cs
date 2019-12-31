namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssFontSizeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fs";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!TryParseFloat(arg, out float size))
                size = context.Style.FontSize;

            context.Section.Scale = size / context.Document.DefaultFontSize;
        }
    }
}
