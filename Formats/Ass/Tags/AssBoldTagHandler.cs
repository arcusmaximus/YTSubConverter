namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssBoldTagHandler : AssTagHandlerBase
    {
        public override string Tag => "b";

        public override void Handle(AssTagContext context, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                context.Section.Bold = context.Style.Bold;
                return;
            }

            if (!int.TryParse(arg, out int value))
            {
                context.Section.Bold = false;
                return;
            }

            if (value == 0)
                context.Section.Bold = false;
            else if (value == 1)
                context.Section.Bold = true;
            else
                context.Section.Bold = context.Style.Bold;
        }
    }
}
