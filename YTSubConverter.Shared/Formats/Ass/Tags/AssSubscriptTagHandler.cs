namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssSubscriptTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytsub";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Offset = OffsetType.Subscript;
        }
    }
}
