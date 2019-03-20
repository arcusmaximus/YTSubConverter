namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssResetTagHandler : AssTagHandlerBase
    {
        public override string Tag => "r";

        public override void Handle(AssTagContext context, string arg)
        {
            AssDocument.ApplyStyle(context.Section, context.Style, context.StyleOptions);
        }
    }
}
