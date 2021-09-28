using System.Collections.Generic;
using System.Linq;

namespace Arc.YTSubConverter.Shared.Formats.Ass.KaraokeTypes
{
    public class SimpleKaraokeType : IKaraokeType
    {
        public static readonly SimpleKaraokeType Instance = new SimpleKaraokeType();

        public virtual IEnumerable<AssLine> Apply(AssKaraokeStepContext context)
        {
            foreach (AssSection singingSection in context.SingingSections)
            {
                if (!singingSection.CurrentWordForeColor.IsEmpty)
                    singingSection.ForeColor = singingSection.CurrentWordForeColor;

                if (!singingSection.CurrentWordShadowColor.IsEmpty)
                {
                    foreach (ShadowType shadowType in singingSection.ShadowColors.Keys.ToList())
                    {
                        singingSection.ShadowColors[shadowType] = singingSection.CurrentWordShadowColor;
                    }
                }

                if (!singingSection.CurrentWordOutlineColor.IsEmpty && singingSection.ShadowColors.ContainsKey(ShadowType.Glow))
                    singingSection.ShadowColors[ShadowType.Glow] = singingSection.CurrentWordOutlineColor;
            }

            return new[] { context.StepLine };
        }
    }
}
