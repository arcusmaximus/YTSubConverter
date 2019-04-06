namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssSubscriptTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytsub";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Offset = OffsetType.Subscript;
        }
    }
}
