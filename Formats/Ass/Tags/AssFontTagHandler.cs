namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssFontTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fn";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Font = arg;
        }
    }
}
