namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssItalicTagHandler : AssTagHandlerBase
    {
        public override string Tag => "i";

        public override void Handle(AssTagContext context, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                context.Section.Italic = context.Style.Italic;
                return;
            }

            if (!int.TryParse(arg, out int value))
            {
                context.Section.Italic = false;
                return;
            }

            if (value == 0)
                context.Section.Italic = false;
            else if (value == 1)
                context.Section.Italic = true;
            else
                context.Section.Italic = context.Style.Italic;
        }
    }
}
