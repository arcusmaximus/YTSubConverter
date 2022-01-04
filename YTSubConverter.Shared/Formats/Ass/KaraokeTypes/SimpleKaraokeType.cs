using System.Collections.Generic;
using System.Linq;
using YTSubConverter.Shared.Animations;

namespace YTSubConverter.Shared.Formats.Ass.KaraokeTypes
{
    public class SimpleKaraokeType : IKaraokeType
    {
        public static readonly SimpleKaraokeType Instance = new SimpleKaraokeType();

        public virtual IEnumerable<AssLine> Apply(AssKaraokeStepContext context)
        {
            foreach (AssSection singingSection in context.SingingSections)
            {
                if (!singingSection.CurrentWordForeColor.IsEmpty)
                {
                    singingSection.ForeColor = singingSection.CurrentWordForeColor;
                    singingSection.Animations.RemoveAll(a => a is ForeColorAnimation);
                }

                if (!singingSection.CurrentWordShadowColor.IsEmpty)
                {
                    foreach (ShadowType shadowType in singingSection.ShadowColors.Keys.ToList())
                    {
                        singingSection.ShadowColors[shadowType] = singingSection.CurrentWordShadowColor;
                        singingSection.Animations.RemoveAll(a => a is ShadowColorAnimation shadowAnim && shadowAnim.ShadowType == shadowType);
                    }
                }

                if (!singingSection.CurrentWordOutlineColor.IsEmpty && singingSection.ShadowColors.ContainsKey(ShadowType.Glow))
                {
                    singingSection.ShadowColors[ShadowType.Glow] = singingSection.CurrentWordOutlineColor;
                    singingSection.Animations.RemoveAll(a => a is ShadowColorAnimation shadowAnim && shadowAnim.ShadowType == ShadowType.Glow);
                }
            }

            return new[] { context.StepLine };
        }
    }
}
