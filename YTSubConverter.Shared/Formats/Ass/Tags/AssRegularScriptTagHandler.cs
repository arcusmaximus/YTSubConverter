namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssRegularScriptTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytsur";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Offset = OffsetType.Regular;
        }
    }
}
