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

            (context.Line.HorizontalTextDirection, context.Line.VerticalTextType) = GetVerticalTextType(vertType);
        }

        public static (HorizontalTextDirection, VerticalTextType) GetVerticalTextType(int id)
        {
            return id switch
                   {
                       9 => (HorizontalTextDirection.RightToLeft, VerticalTextType.Positioned),
                       7 => (HorizontalTextDirection.LeftToRight, VerticalTextType.Positioned),
                       1 => (HorizontalTextDirection.LeftToRight, VerticalTextType.Rotated),
                       3 => (HorizontalTextDirection.RightToLeft, VerticalTextType.Rotated),
                       _ => (HorizontalTextDirection.LeftToRight, VerticalTextType.None)
                   };
        }

        public static int GetVerticalTextTypeId(HorizontalTextDirection hor, VerticalTextType ver)
        {
            if (hor == HorizontalTextDirection.LeftToRight)
            {
                return ver switch
                       {
                           VerticalTextType.Positioned => 7,
                           VerticalTextType.Rotated => 1,
                           _ => 0
                       };
            }
            else
            {
                return ver switch
                       {
                           VerticalTextType.Positioned => 9,
                           VerticalTextType.Rotated => 3,
                           _ => 0
                       };
            }
        }
    }
}
