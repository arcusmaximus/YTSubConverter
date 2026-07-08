using System;
using System.Collections.Generic;
using System.Linq;
using YTSubConverter.Shared.Animations;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass.KaraokeTypes
{
    public class GlitchKaraokeType : SimpleKaraokeType
    {
        private static readonly CharacterRange[][] LanguageCharacterRanges =
        [
            [new CharacterRange('A', 'Z'), new CharacterRange('a', 'z')],
            [CharacterRange.IdeographRange, CharacterRange.IdeographExtensionRange, CharacterRange.IdeographCompatibilityRange],
            [CharacterRange.HiraganaRange],
            [CharacterRange.KatakanaRange],
            [CharacterRange.HangulRange]
        ];

        private static readonly CharacterRange[] RandomCharacterRanges =
            [
                new('\x2300', '\x231A'),
                new('\x231C', '\x23E1')
            ];

        public override IEnumerable<AssLine> Apply(AssKaraokeStepContext context)
        {
            AssSection singingSection = context.SingingSections.LastOrDefault(s => s.RubyPart == RubyPart.None || s.RubyPart == RubyPart.Base);
            if (singingSection == null || singingSection.Text.Length == 0)
                return [context.StepLine];

            base.Apply(context);
            DateTime glitchEndTime = TimeUtil.FrameToEndTime(TimeUtil.StartTimeToFrame(context.StepLine.Start) + 1);
            CharacterRange[] charRanges = GetGlitchKaraokeCharacterRanges(singingSection.Text[0]);
            singingSection.Animations.Add(new GlitchingCharAnimation(context.StepLine.Start, glitchEndTime, charRanges));
            return [context.StepLine];
        }

        private static CharacterRange[] GetGlitchKaraokeCharacterRanges(char c)
        {
            foreach (CharacterRange[] ranges in LanguageCharacterRanges)
            {
                if (ranges.Any(r => r.Contains(c)))
                    return ranges;
            }

            return RandomCharacterRanges;
        }
    }
}
