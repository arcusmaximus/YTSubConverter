namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssUnderlineTagHandler : AssTagHandlerBase
    {
        public override string Tag => "u";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                context.Section.Underline = context.InitialStyle.Underline;
                return;
            }

            if (!TryParseInt(arg, out int value))
            {
                context.Section.Underline = false;
                return;
            }

            if (value == 0)
                context.Section.Underline = false;
            else if (value == 1)
                context.Section.Underline = true;
            else
                context.Section.Underline = context.InitialStyle.Underline;
        }
    }
}
