namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssResetTagHandler : AssTagHandlerBase
    {
        public override string Tag => "r";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            context.Style = context.Document.GetStyle(arg) ?? context.InitialStyle;
            context.StyleOptions = context.Document.GetStyleOptions(arg) ?? context.InitialStyleOptions;
            AssDocument.ApplyStyle(context.Section, context.Style, context.StyleOptions);
            context.Section.Scale = 1;
            context.Section.Offset = OffsetType.Regular;
            context.Section.RubyPosition = RubyPosition.None;
            context.Section.Animations.Clear();
        }
    }
}
