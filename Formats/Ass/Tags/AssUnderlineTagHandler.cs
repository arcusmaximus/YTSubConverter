namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssUnderlineTagHandler : AssTagHandlerBase
    {
        public override string Tag => "u";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Underline = arg != "0";
        }
    }
}
