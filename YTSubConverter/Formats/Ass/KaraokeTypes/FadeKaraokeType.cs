using System;
using System.Collections.Generic;
using System.Drawing;
using Arc.YTSubConverter.Animations;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass.KaraokeTypes
{
    internal class FadeKaraokeType : IKaraokeType
    {
        public IEnumerable<AssLine> Apply(AssKaraokeStepContext context)
        {
            ApplyFadeInKaraokeEffect(context.StepLine, context.SingingSections);
            ApplyFadeOutKaraokeEffect(context.OriginalLine, context.StepLine, context.ActiveSectionsPerStep, context.StepIndex);
            return new[] { context.StepLine };
        }

        private static void ApplyFadeInKaraokeEffect(AssLine stepLine, List<AssSection> singingSections)
        {
            DateTime fadeEndTime = TimeUtil.Min(stepLine.Start.AddMilliseconds(500), stepLine.End);

            foreach (AssSection singingSection in singingSections)
            {
                if (singingSection.CurrentWordForeColor.IsEmpty)
                {
                    if (singingSection.ForeColor != singingSection.SecondaryColor)
                        singingSection.Animations.Add(new ForeColorAnimation(stepLine.Start, singingSection.SecondaryColor, fadeEndTime, singingSection.ForeColor));
                }
                else
                {
                    if (singingSection.CurrentWordForeColor != singingSection.SecondaryColor)
                        singingSection.Animations.Add(new ForeColorAnimation(stepLine.Start, singingSection.SecondaryColor, fadeEndTime, singingSection.CurrentWordForeColor));
                }

                if (!singingSection.CurrentWordShadowColor.IsEmpty)
                {
                    foreach (KeyValuePair<ShadowType, Color> shadowColor in singingSection.ShadowColors)
                    {
                        if (singingSection.CurrentWordShadowColor != shadowColor.Value)
                            singingSection.Animations.Add(new ShadowColorAnimation(shadowColor.Key, stepLine.Start, shadowColor.Value, fadeEndTime, singingSection.CurrentWordShadowColor));
                    }
                }

                if (!singingSection.CurrentWordOutlineColor.IsEmpty && singingSection.CurrentWordOutlineColor != singingSection.ShadowColors.GetOrDefault(ShadowType.Glow))
                {
                    singingSection.Animations.Add(new ShadowColorAnimation(
                        ShadowType.Glow, stepLine.Start, singingSection.ShadowColors[ShadowType.Glow], fadeEndTime, singingSection.CurrentWordOutlineColor));
                }
            }
        }

        private static void ApplyFadeOutKaraokeEffect(AssLine originalLine, AssLine stepLine, SortedList<TimeSpan, int> activeSectionsPerStep, int stepIdx)
        {
            int stepFirstSectionIdx = 0;
            for (int prevStepIdx = 0; prevStepIdx < stepIdx; prevStepIdx++)
            {
                DateTime fadeStartTime = originalLine.Start + activeSectionsPerStep.Keys[prevStepIdx + 1];
                DateTime fadeEndTime = fadeStartTime.AddMilliseconds(1000);
                int stepLastSectionIdx = activeSectionsPerStep.Values[prevStepIdx] - 1;
                for (int sectionIdx = stepFirstSectionIdx; sectionIdx <= stepLastSectionIdx; sectionIdx++)
                {
                    AssSection section = (AssSection)stepLine.Sections[sectionIdx];
                    if (!section.CurrentWordForeColor.IsEmpty && section.CurrentWordForeColor != section.ForeColor)
                        section.Animations.Add(new ForeColorAnimation(fadeStartTime, section.CurrentWordForeColor, fadeEndTime, section.ForeColor));

                    if (!section.CurrentWordShadowColor.IsEmpty)
                    {
                        foreach (KeyValuePair<ShadowType, Color> shadowColor in section.ShadowColors)
                        {
                            if (section.CurrentWordShadowColor != shadowColor.Value)
                                section.Animations.Add(new ShadowColorAnimation(shadowColor.Key, fadeStartTime, section.CurrentWordShadowColor, fadeEndTime, shadowColor.Value));
                        }
                    }

                    if (!section.CurrentWordOutlineColor.IsEmpty && section.CurrentWordOutlineColor != section.ShadowColors.GetOrDefault(ShadowType.Glow))
                        section.Animations.Add(new ShadowColorAnimation(ShadowType.Glow, fadeStartTime, section.CurrentWordOutlineColor, fadeEndTime, section.ShadowColors[ShadowType.Glow]));
                }

                stepFirstSectionIdx = stepLastSectionIdx + 1;
            }
        }
    }
}
