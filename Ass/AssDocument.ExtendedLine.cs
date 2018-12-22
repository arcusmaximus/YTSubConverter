using System;
using System.Drawing;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Ass
{
    internal partial class AssDocument
    {
        private class ExtendedLine : Line
        {
            public ExtendedLine(DateTime start, DateTime end)
                : base(start, end)
            {
            }

            public bool UseFade
            {
                get;
                set;
            }

            public int FadeInitialAlpha
            {
                get;
                set;
            }

            public int FadeMidAlpha
            {
                get;
                set;
            }

            public int FadeFinalAlpha
            {
                get;
                set;
            }

            public DateTime FadeInStartTime
            {
                get;
                set;
            }

            public DateTime FadeInEndTime
            {
                get;
                set;
            }

            public DateTime FadeOutStartTime
            {
                get;
                set;
            }

            public DateTime FadeOutEndTime
            {
                get;
                set;
            }

            public void MultiplySectionAlphas(float factor)
            {
                foreach (Section section in Sections)
                {
                    section.ForeColor = MultiplyColorAlpha(section.ForeColor, factor);
                    section.BackColor = MultiplyColorAlpha(section.BackColor, factor);
                    section.ShadowColor = MultiplyColorAlpha(section.ShadowColor, factor);
                }
            }

            private static Color MultiplyColorAlpha(Color color, float factor)
            {
                if (color.IsEmpty)
                    return color;

                return ColorUtil.ChangeColorAlpha(color, (int)(color.A * factor));
            }

            public override object Clone()
            {
                ExtendedLine newLine = new ExtendedLine(Start, End);
                newLine.Assign(this);
                return newLine;
            }

            protected override void Assign(Line line)
            {
                base.Assign(line);

                ExtendedLine extendedLine = (ExtendedLine)line;
                UseFade = extendedLine.UseFade;
                FadeInitialAlpha = extendedLine.FadeInitialAlpha;
                FadeMidAlpha = extendedLine.FadeMidAlpha;
                FadeFinalAlpha = extendedLine.FadeFinalAlpha;
                FadeInStartTime = extendedLine.FadeInStartTime;
                FadeInEndTime = extendedLine.FadeInEndTime;
                FadeOutStartTime = extendedLine.FadeOutStartTime;
                FadeOutEndTime = extendedLine.FadeOutEndTime;
            }
        }
    }
}
