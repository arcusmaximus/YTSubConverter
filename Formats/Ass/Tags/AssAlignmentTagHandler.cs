namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssAlignmentTagHandler : AssTagHandlerBase
    {
        public override string Tag => "an";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!int.TryParse(arg, out int alignment))
                return;

            context.Line.AnchorPoint = AssDocument.GetAnchorPointFromAlignment(alignment);
        }
    }
}
