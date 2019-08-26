namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssAlignmentTagHandler : AssTagHandlerBase
    {
        public override string Tag => "an";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            int alignment = ParseInt(arg);
            if (alignment >= 1 && alignment <= 9)
                context.Line.AnchorPoint = AssDocument.GetAnchorPointFromAlignment(alignment);
        }
    }
}
