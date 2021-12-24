namespace YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssRubyTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytruby";

        public override bool AffectsWholeLine => false;

        public override void Handle(AssTagContext context, string arg)
        {
            int.TryParse(arg, out int rubyPos);
            context.Section.RubyPart = rubyPos == 2 ? RubyPart.TextAfter : RubyPart.TextBefore;
        }
    }
}
