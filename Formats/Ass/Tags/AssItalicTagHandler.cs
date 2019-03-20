namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssItalicTagHandler : AssTagHandlerBase
    {
        public override string Tag => "i";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Italic = arg != "0";
        }
    }
}
