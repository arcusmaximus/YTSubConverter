namespace Arc.YTSubConverter.Shared.Formats.Ass.Tags
{
    internal class AssHorizontalTextDirectionTag : AssTagHandlerBase
    {
        public override string Tag => "ytdir";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            if (!int.TryParse(arg, out int direction))
                direction = 6;

            context.Line.HorizontalTextDirection = GetHorizontalTextDirection(direction);
        }

        public static HorizontalTextDirection GetHorizontalTextDirection(int directionId)
        {
            return directionId switch
                   {
                       4 => HorizontalTextDirection.RightToLeft,
                       _ => HorizontalTextDirection.LeftToRight
                   };
        }

        public static int GetHorizontalTextDirectionId(HorizontalTextDirection direction)
        {
            return direction switch
                   {
                       HorizontalTextDirection.RightToLeft => 4,
                       _ => 6
                   };
        }
    }
}
