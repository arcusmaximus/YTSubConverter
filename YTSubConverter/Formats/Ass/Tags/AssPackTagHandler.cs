namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssPackTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytpack";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Section.Packed = arg != "0";
        }
    }
}
