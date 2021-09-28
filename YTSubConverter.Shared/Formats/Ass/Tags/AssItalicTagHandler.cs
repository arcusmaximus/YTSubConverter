namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssItalicTagHandler : AssTagHandlerBase
    {
        public override string Tag => "i";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                context.Section.Italic = context.InitialStyle.Italic;
                return;
            }

            if (!TryParseInt(arg, out int value))
            {
                context.Section.Italic = false;
                return;
            }

            if (value == 0)
                context.Section.Italic = false;
            else if (value == 1)
                context.Section.Italic = true;
            else
                context.Section.Italic = context.InitialStyle.Italic;
        }
    }
}
