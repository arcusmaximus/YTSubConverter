using System.Collections.Generic;
using System.Drawing;

namespace Arc.YTSubConverter.Formats.Ass.Tags
{
    internal class AssPositionTagHandler : AssTagHandlerBase
    {
        public override string Tag => "pos";

        public override bool AffectsWholeLine => true;

        public override void Handle(AssTagContext context, string arg)
        {
            List<float> coords = ParseFloatList(arg);
            if (coords == null || coords.Count != 2 || context.Line.Position != null)
                return;

            context.Line.Position = new PointF(coords[0], coords[1]);
        }
    }
}
