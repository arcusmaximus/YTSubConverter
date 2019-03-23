using System.Linq;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssResetTagHandler : AssTagHandlerBase
    {
        public override string Tag => "r";

        public override void Handle(AssTagContext context, string arg)
        {
            AssStyle style = string.IsNullOrEmpty(arg) ? context.Style : context.Document.Styles.FirstOrDefault(s => s.Name == arg) ?? context.Style;
            AssDocument.ApplyStyle(context.Section, style, context.StyleOptions);
        }
    }
}
