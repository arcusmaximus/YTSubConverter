namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssFontTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fn";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Font = !string.IsNullOrEmpty(arg) ? arg : context.Style.Font;
        }
    }
}
