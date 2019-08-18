using System.Globalization;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssFontSizeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "fs";

        public override void Handle(AssTagContext context, string arg)
        {
            if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out float size))
                size = context.Style.FontSize;

            context.Section.Scale = size / context.Style.FontSize;
        }
    }
}
