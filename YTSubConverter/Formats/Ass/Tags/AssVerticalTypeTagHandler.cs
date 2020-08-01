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

            context.Line.VerticalTextType = GetVerticalTextType(vertType);
        }

        public static VerticalTextType GetVerticalTextType(int id)
        {
            return id switch
                   {
                       9 => VerticalTextType.VerticalRtl,
                       7 => VerticalTextType.VerticalLtr,
                       1 => VerticalTextType.RotatedLtr,
                       3 => VerticalTextType.RotatedRtl,
                       _ => VerticalTextType.None
                   };
        }

        public static int GetVerticalTextTypeId(VerticalTextType type)
        {
            return type switch
                   {
                       VerticalTextType.VerticalRtl => 9,
                       VerticalTextType.VerticalLtr => 7,
                       VerticalTextType.RotatedLtr => 1,
                       VerticalTextType.RotatedRtl => 3,
                       _ => 0
                   };
        }
    }
}
