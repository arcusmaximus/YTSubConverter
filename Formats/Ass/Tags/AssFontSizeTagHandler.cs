namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssFontSizeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fs";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!int.TryParse(arg, out int size))
                size = context.Style.FontSize;

            context.Section.Scale = (float)size / context.Style.FontSize;
        }
    }
}
