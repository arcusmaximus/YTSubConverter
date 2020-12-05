using System;
using Arc.YTSubConverter.Formats;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Animations
{
    public class GlitchingCharAnimation : Animation
    {
        private readonly Random _random;
        private readonly CharacterRange[] _charRanges;

        public GlitchingCharAnimation(DateTime startTime, DateTime endTime, params CharacterRange[] charRanges)
            : base(startTime, endTime, 1)
        {
            _random = new Random((int)(startTime - SubtitleDocument.TimeBase).TotalMilliseconds);
            _charRanges = charRanges;
        }

        public override bool AffectsPast => false;

        public override bool AffectsText => true;

        public override void Apply(AssLine line, AssSection section, float t)
        {
            if (t > 0 && t < 1)
                section.Text = GetRandomChar().ToString();
        }

        private char GetRandomChar()
        {
            CharacterRange range = _charRanges[_random.Next(_charRanges.Length)];
            return (char)(range.Start + _random.Next(range.End - range.Start));
        }

        public override object Clone()
        {
            return new GlitchingCharAnimation(StartTime, EndTime, _charRanges);
        }
    }
}
