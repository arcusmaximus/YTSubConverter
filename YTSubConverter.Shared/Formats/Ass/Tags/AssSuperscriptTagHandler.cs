namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssSuperscriptTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytsup";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Offset = OffsetType.Superscript;
        }
    }
}
