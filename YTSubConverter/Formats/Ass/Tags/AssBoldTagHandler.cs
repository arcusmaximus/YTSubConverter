namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssBoldTagHandler : AssTagHandlerBase
    {
        public override string Tag => "b";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                context.Section.Bold = context.InitialStyle.Bold;
                return;
            }

            if (!TryParseInt(arg, out int value))
            {
                context.Section.Bold = false;
                return;
            }

            if (value == 0)
                context.Section.Bold = false;
            else if (value == 1)
                context.Section.Bold = true;
            else
                context.Section.Bold = context.InitialStyle.Bold;
        }
    }
}
