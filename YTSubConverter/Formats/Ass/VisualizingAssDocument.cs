using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class VisualizingAssDocument : AssDocument
    {
        public VisualizingAssDocument(string filePath, List<AssStyleOptions> styleOptions = null)
            : base(filePath, styleOptions)
        {
        }

        public VisualizingAssDocument(SubtitleDocument doc)
            : base(doc)
        {
            EmulateKaraokeForLinesWithBackgroundOrShadow();
        }

        /// <summary>
        /// Unlike YouTube, where the native karaoke feature hides the unsung parts completely (text/background box/shadow),
        /// the equivalent in Aegisub (\k with a transparent secondary color) only hides the text. This means we need to
        /// switch to emulated karaoke (duplicated lines without \k) for visual correctness.
        /// </summary>
        private void EmulateKaraokeForLinesWithBackgroundOrShadow()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                AssLine line = (AssLine)Lines[i];
                if (line.Sections.Cast<AssSection>().All(s => s.Duration == TimeSpan.Zero) ||
                    line.Sections.All(s => s.BackColor.A == 0 && s.ShadowColors.Count == 0))
                    continue;

                Lines.RemoveAt(i);
                Lines.InsertRange(i, CreateEmulatedKaraokeLines(line));
            }
        }

        /// <summary>
        /// Because Aegisub uses a nonstandard definition of font size, we need to apply a font-dependent
        /// conversion factor to get the same visual size as in, say, a browser.
        /// </summary>
        protected override float ScaleToLineHeight(string font, float scale)
        {
            float defaultFontSize = FontSizeMapper.LineHeightToFontSize(DefaultStyle.Font, DefaultStyle.LineHeight);
            float fontSize = defaultFontSize * scale;
            return FontSizeMapper.FontSizeToLineHeight(font, fontSize);
        }

        protected override void WriteLine(AssLine line, StreamWriter writer)
        {
            MoveLineBreaksToSeparateSections(line);
            EmulateBorders(line);
            ProtectWhitespace(line);

            if (line.Sections.All(s => s.ShadowColors.Count == 0))
            {
                base.WriteLine(line, writer);
                return;
            }

            if (line.Sections.Any(s => s.BackColor.A > 0))
                base.WriteLine(CreateBackgroundVisualizationLine(line), writer);

            foreach (AssLine shadowLine in CreateShadowVisualizationLines(line))
            {
                base.WriteLine(shadowLine, writer);
            }

            base.WriteLine(CreateTextVisualizationLine(line), writer);
        }

        // Aegisub's background boxes can have padding just like on YouTube, and the horizontal and vertical padding
        // can even be different. The big downside of this feature, however, is that background boxes of adjacent
        // sections overlap each other which looks like a mess. Therefore we don't use it at all (all the styles
        // have an outline thickness of 0.01) and instead add a space at the start and end of each line of text
        // to emulate the YouTube horizontal padding manually.
        private static void EmulateBorders(AssLine line)
        {
            for (int i = 0; i < line.Sections.Count; i++)
            {
                AssSection section = (AssSection)line.Sections[i];
                if (Regex.IsMatch(section.Text, @"^[\r\n]+$"))
                    continue;

                if (i == 0 || line.Sections[i - 1].Text.EndsWith("\r\n"))
                    section.Text = " " + section.Text;

                if (i == line.Sections.Count - 1 || line.Sections[i + 1].Text.StartsWith("\r\n"))
                    section.Text = section.Text + " ";
            }
        }

        // Aegisub doesn't display the background box for certain parts of a subtitle:
        // - Whitespace at the start of a line of text
        // - Whitespace at the end of a line of text
        // - Sections that consist only of whitespace
        // YouTube displays the background box across the entire subtitle, however,
        // so for an accurate visualization, we need to enclose whitespace blocks in
        // other characters to "protect" them.
        private static void ProtectWhitespace(AssLine line)
        {
            for (int i = line.Sections.Count - 1; i >= 0; i--)
            {
                AssSection section = (AssSection)line.Sections[i];
                if (Regex.IsMatch(section.Text, @"^[\r\n]+$"))
                    continue;

                Match match = Regex.Match(section.Text, @"^([ \xA0]*)(.*?)([ \xA0]*)$");
                if (match.Groups[1].Length == section.Text.Length)
                {
                    // If the entire section is just whitespace, we have to use a non-whitespace character to make its background visible
                    section.Text = CreateProtectedWhitespaceString(section.Text.Length, ".");
                    section.ForeColor = ColorUtil.ChangeColorAlpha(section.ForeColor, 0);
                    section.ShadowColors.Clear();
                }
                else
                {
                    // Otherwise, we can use non-breaking spaces (which don't normally work, but do work in our case because we later write
                    // them out using Aegisub's \h sequence)
                    section.Text = CreateProtectedWhitespaceString(match.Groups[1].Length, "\xA0") + match.Groups[2].Value + CreateProtectedWhitespaceString(match.Groups[3].Length, "\xA0");
                }
            }
        }

        private static string CreateProtectedWhitespaceString(int length, string protection)
        {
            if (length == 0)
                return string.Empty;

            if (length == 1)
                return protection;

            return protection + new string(' ', length - 2) + protection;
        }

        private AssLine CreateBackgroundVisualizationLine(AssLine originalLine)
        {
            AssLine backgroundLine = (AssLine)originalLine.Clone();
            foreach (AssSection section in backgroundLine.Sections)
            {
                section.ForeColor = Color.Empty;
                section.SecondaryColor = Color.Empty;
                section.ShadowColors.Clear();
            }
            return backgroundLine;
        }

        private IEnumerable<AssLine> CreateShadowVisualizationLines(AssLine originalLine)
        {
            if (originalLine.Sections.Any(s => s.ShadowColors.ContainsKey(ShadowType.SoftShadow)))
            {
                for (float blur = 4; blur >= 2; blur--)
                {
                    yield return CreateShadowVisualizationLine(originalLine, s => s.ShadowColors.GetOrDefault(ShadowType.SoftShadow), 2, blur);
                }
            }

            if (originalLine.Sections.Any(s => s.ShadowColors.ContainsKey(ShadowType.HardShadow)))
            {
                for (int offset = 3; offset >= 1; offset--)
                {
                    yield return CreateShadowVisualizationLine(originalLine, s => s.ShadowColors.GetOrDefault(ShadowType.HardShadow), offset, 0);
                }
            }

            if (originalLine.Sections.Any(s => s.ShadowColors.ContainsKey(ShadowType.Bevel)))
            {
                yield return CreateShadowVisualizationLine(originalLine, s => s.ShadowColors.GetOrDefault(ShadowType.Bevel), -1, 0);

                yield return CreateShadowVisualizationLine(
                    originalLine,
                    s =>
                    {
                        Color color = s.ShadowColors.GetOrDefault(ShadowType.Bevel);
                        if (color.IsEmpty)
                            return Color.Empty;

                        if (color.R == 0x22 && color.G == 0x22 & color.B == 0x22)
                            return Color.FromArgb(color.A, 0xCC, 0xCC, 0xCC);

                        return color;
                    },
                    1,
                    0
                );
            }

            if (originalLine.Sections.Any(s => s.ShadowColors.ContainsKey(ShadowType.Glow)))
            {
                for (float blur = 2; blur >= 1; blur -= 0.5f)
                {
                    yield return CreateShadowVisualizationLine(originalLine, s => s.ShadowColors.GetOrDefault(ShadowType.Glow), 0, blur);
                }
            }
        }

        private AssLine CreateShadowVisualizationLine(AssLine originalLine, Func<AssSection, Color> getShadowColor, float positionOffset, float blur)
        {
            AssLine shadowLine = (AssLine)originalLine.Clone();

            if (positionOffset != 0)
            {
                PointF position = shadowLine.Position ?? GetDefaultPosition(shadowLine.AnchorPoint);
                shadowLine.Position = new PointF(position.X + positionOffset, position.Y + positionOffset);
            }

            foreach (AssSection shadowSection in shadowLine.Sections)
            {
                shadowSection.ForeColor = getShadowColor(shadowSection);
                shadowSection.BackColor = Color.Empty;
                shadowSection.ShadowColors.Clear();
                shadowSection.Blur = blur;
            }
            return shadowLine;
        }

        private AssLine CreateTextVisualizationLine(AssLine originalLine)
        {
            AssLine textLine = (AssLine)originalLine.Clone();
            foreach (AssSection textSection in textLine.Sections)
            {
                textSection.BackColor = Color.Empty;
                textSection.ShadowColors.Clear();
            }
            return textLine;
        }
    }
}
