namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssRegularScriptTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytsur";

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Offset = OffsetType.Regular;
        }
    }
}
