namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssRubyTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytruby";

        public override void Handle(AssTagContext context, string arg)
        {
            int.TryParse(arg, out int rubyPos);
            context.Line.RubyPosition = rubyPos == 2 ? RubyPosition.Below : RubyPosition.Above;
        }
    }
}
