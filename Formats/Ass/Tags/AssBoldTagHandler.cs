namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssBoldTagHandler : AssTagHandlerBase
    {
        public override string Tag => "b";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Bold = arg != "0";
        }
    }
}
