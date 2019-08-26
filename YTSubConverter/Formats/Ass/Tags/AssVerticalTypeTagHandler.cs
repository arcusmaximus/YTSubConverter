namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssVerticalTypeTagHandler : AssTagHandlerBase
    {
        public override string Tag => "ytvert";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!int.TryParse(arg, out int vertType))
                vertType = 9;

            switch (vertType)
            {
                case 9:
                    context.Line.VerticalTextType = VerticalTextType.VerticalRtl;
                    break;

                case 7:
                    context.Line.VerticalTextType = VerticalTextType.VerticalLtr;
                    break;

                case 1:
                    context.Line.VerticalTextType = VerticalTextType.RotatedLtr;
                    break;

                case 3:
                    context.Line.VerticalTextType = VerticalTextType.RotatedRtl;
                    break;
            }
        }
    }
}
