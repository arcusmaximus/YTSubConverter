using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats
{
    internal class YttDocument : SubtitleDocument
    {
        /// <summary>
        /// Defines the (approximate) delay between the specified subtitle appearance time and the time it actually appears.
        /// </summary>
        private const int SubtitleDelayMs = 60;

        public YttDocument(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            foreach (XmlElement parElem in doc.SelectNodes("/timedtext/body/p"))
            {
                int startMs = int.Parse(parElem.GetAttribute("t")) + SubtitleDelayMs;
                int durationMs = int.Parse(parElem.GetAttribute("d"));

                DateTime start = TimeBase.AddMilliseconds(startMs);
                DateTime end = start.AddMilliseconds(durationMs);
                Line line = new Line(start, end, parElem.InnerText);
                Lines.Add(line);
            }
        }

        public YttDocument(SubtitleDocument doc)
            : base(doc)
        {
            Lines.RemoveAll(l => !l.Sections.Any(s => s.Text.Length > 0));

            // A gap of even one millisecond can cause flickering
            CloseGaps();
        }

        public override void Save(string filePath)
        {
            ApplyWorkarounds();

            ExtractAttributes(
                Lines,
                new LinePositionComparer(),
                out Dictionary<Line, int> positionIds,
                out List<Line> positions
            );

            ExtractAttributes(
                Lines.SelectMany(l => l.Sections),
                new SectionFormatComparer(),
                out Dictionary<Section, int> penIds,
                out List<Section> pens
            );

            using (XmlWriter writer = XmlWriter.Create(filePath))
            {
                writer.WriteStartElement("timedtext");
                writer.WriteAttributeString("format", "3");

                WriteHead(writer, positions, pens);
                WriteBody(writer, positionIds, penIds);

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Applies various workarounds for bugs and limitations in YouTube's subtitles.
        /// </summary>
        private void ApplyWorkarounds()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                HardenSpaces(i);
                PrepareForMultiForeground(i);
                PrepareForMultiNonForeground(i);
                
                i += ExpandLineForHighAlpha(i) - 1;
                i += ExpandLineForMultiShadows(i) - 1;
                i += ExpandLineForColoring(i) - 1;
            }
        }

        /// <summary>
        /// Sequences of multiple spaces get collapsed into a single space in browsers -> replace by non-breaking spaces.
        /// (Useful for expanding the background box to cover up on-screen text)
        /// </summary>
        private void HardenSpaces(int lineIndex)
        {
            foreach (Section section in Lines[lineIndex].Sections)
            {
                section.Text = Regex.Replace(section.Text, @"  +", m => new string(' ', m.Value.Length));
            }
        }

        /// <summary>
        /// Once you read how the multicolor workaround works (see <see cref="ExpandLineForColoring"/>),
        /// you'll notice that it doesn't work in a specific scenario: text that is centered and
        /// has a section boundary (= style change) between two line breaks. For example, if we
        /// have the centered text "Red Green" (where each word has its respective color),
        /// <see cref="ExpandLineForColoring"/> would produce the following (+ indicates the subtitle's anchor point):
        ///       Layer 1 (#00FF00): |   Red+Green   |
        ///       Layer 2 (#FF0000): |     R+d       |
        ///
        /// This is of course not what we want - the two "Red"s should overlap. We have to make
        /// the text left-aligned (and change the position to compensate).
        ///       Layer 1 (#00FF00): |  +Red Green   |
        ///       Layer 2 (#FF0000): |  +Red         |
        ///
        /// In the case of text with line breaks, we have an additional complication: only the longest
        /// line of text would keep its position while the others would shift.
        ///
        ///      Original:          |   This is a+multiline   |
        ///                         |           sub.          |
        ///
        ///      Adjusted:          |  +This is a multiline   |
        ///                         |   sub.                  |
        ///
        /// To work around *this*, we need to split the line so each line of text can be positioned individually:
        ///                         |  +This is a multiline   |
        ///                         |          +sub.          |
        /// </summary>
        private void PrepareForMultiForeground(int lineIndex)
        {
            Line line = Lines[lineIndex];
            AnchorPoint anchorPoint = line.AnchorPoint ?? AnchorPoint.BottomCenter;
            if (!AnchorPointUtil.IsCenterAligned(anchorPoint) || !HasSectionBorderBetweenLineBreaks(line))
                return;

            int numLines = SplitOnLineBreaks(lineIndex);
            for (int i = 0; i < numLines; i++)
            {
                MakeLeftAligned(Lines[lineIndex + i]);
            }
        }

        private bool HasSectionBorderBetweenLineBreaks(Line line)
        {
            for (int i = 0; i < line.Sections.Count - 1; i++)
            {
                if (!line.Sections[i].Text.EndsWith("\r\n") && !line.Sections[i + 1].Text.StartsWith("\r\n"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Text is typically opaque and can be overlaid without problems (no line splitting required).
        /// For other attributes like background color and bold/italic/underline, however, we can't do this.
        /// </summary>
        private void PrepareForMultiNonForeground(int lineIndex)
        {
            Line line = Lines[lineIndex];
            if (line.Sections.Select(s => s.BackColor).Distinct().Count() > 1 ||
                line.Sections.Select(s => s.Bold).Distinct().Count() > 1 ||
                line.Sections.Select(s => s.Italic).Distinct().Count() > 1 ||
                line.Sections.Select(s => s.Underline).Distinct().Count() > 1)
            {
                SplitOnLineBreaks(lineIndex);
            }
        }

        /// <summary>
        /// Splits one line into multiple lines at the line breaks. (Currently only one line break is supported)
        /// </summary>
        private int SplitOnLineBreaks(int lineIndex)
        {
            Line originalLine = Lines[lineIndex];
            List<List<Section>> subLines = GetSubLines(originalLine);
            if (subLines.Count == 1)
                return 1;

            if (subLines.Count > 2)
                throw new NotSupportedException(string.Format(Resources.CenteredOrRightAlignedTextCanHaveAtMostOneLineBreak0, originalLine.Text));

            List<Line> newLines = new List<Line>();
            foreach (List<Section> sections in subLines)
            {
                Line newLine = (Line)originalLine.Clone();
                newLine.Sections.Clear();
                newLine.Sections.AddRange(sections);
                newLines.Add(newLine);
            }

            AnchorPoint anchorPoint = originalLine.AnchorPoint ?? AnchorPoint.BottomCenter;
            if (AnchorPointUtil.IsTopAligned(anchorPoint))
            {
                PointF pos = originalLine.Position ?? GetDefaultPosition(anchorPoint);
                pos.Y += (int)Math.Ceiling(VideoDimensions.Height * 0.05f);
                newLines[0].Position = newLines[1].Position = pos;
                newLines[0].AnchorPoint = AnchorPointUtil.GetVerticalOpposite(anchorPoint);
            }
            else if (AnchorPointUtil.IsBottomAligned(anchorPoint))
            {
                PointF pos = originalLine.Position ?? GetDefaultPosition(anchorPoint);
                pos.Y -= (int)Math.Ceiling(VideoDimensions.Height * 0.05f);
                newLines[0].Position = newLines[1].Position = pos;
                newLines[1].AnchorPoint = AnchorPointUtil.GetVerticalOpposite(anchorPoint);
            }
            else
            {
                throw new NotSupportedException(string.Format(Resources.CenteredOrRightAlignedTextWithLineBreaksMustBeTopOrBottomAligned0, originalLine.Text));
            }

            Lines.RemoveAt(lineIndex);
            for (int i = 0; i < newLines.Count; i++)
            {
                Lines.Insert(lineIndex + i, newLines[i]);
            }
            return newLines.Count;
        }

        private static List<List<Section>> GetSubLines(Line line)
        {
            List<List<Section>> subLines = new List<List<Section>>();
            List<Section> currentSubLine = null;
            foreach (Section section in line.Sections)
            {
                int start = 0;
                int end;
                while (start < section.Text.Length)
                {
                    end = section.Text.IndexOf("\r\n", start);
                    if (end < 0)
                        end = section.Text.Length;

                    if (currentSubLine == null)
                    {
                        currentSubLine = new List<Section>();
                        subLines.Add(currentSubLine);
                    }

                    if (end > start)
                    {
                        Section newSection = (Section)section.Clone();
                        newSection.Text = section.Text.Substring(start, end - start);
                        currentSubLine.Add(newSection);
                    }

                    if (end < section.Text.Length)
                        currentSubLine = null;

                    start = end + 2;
                }
            }
            return subLines;
        }

        private void MakeLeftAligned(Line line)
        {
            AnchorPoint anchorPoint = line.AnchorPoint ?? AnchorPoint.BottomCenter;
            if (AnchorPointUtil.IsLeftAligned(anchorPoint))
                return;

            if (line.Sections.Any(s => s.Text.Contains("\r\n")))
                throw new NotSupportedException(string.Format(Resources.CantLeftAlignTextWithLineBreaks0, line.Text));

            line.AnchorPoint = AnchorPointUtil.AlignLeft(anchorPoint);

            int widthAt720p = TextUtil.MeasureWidth(line.Text, line.Sections[0].Font, 24, line.Sections[0].Bold, line.Sections[0].Italic, 3);
            float widthAtVideoSize = widthAt720p / 1280f * VideoDimensions.Width;

            PointF position = line.Position ?? GetDefaultPosition(anchorPoint);
            if (AnchorPointUtil.IsCenterAligned(anchorPoint))
                line.Position = new PointF(position.X - widthAtVideoSize / 2, position.Y);
            else
                line.Position = new PointF(position.X - widthAtVideoSize, position.Y);
        }

        /// <summary>
        /// For reasons only YouTube's developers understand, background opacity is capped to a maximum of 75% on PC
        /// (even though it's locked to 100% on Android). So, if the subtitle file wants a higher opacity, we stack
        /// lines on top of each other until we reach that goal.
        /// </summary>
        private int ExpandLineForHighAlpha(int lineIndex)
        {
            Line line = Lines[lineIndex];

            // Find the desired background opacity, limiting it to 250 to keep line duplication within reasonable bounds.
            // (As we progress, we need more and more duplications for less and less opacity gain)
            int targetAlpha = Math.Min(line.Sections.Max(s => s.BackColor.A), (byte)250);

            // If it's already 75% or less, we don't need any duplication.
            if (targetAlpha <= 191)
                return 1;

            foreach (Section section in line.Sections)
            {
                section.BackColor = ColorUtil.ChangeColorAlpha(section.BackColor, 191);
            }

            int numReplacementLines = 1;
            int reachedAlpha = 191;
            while (reachedAlpha < targetAlpha)
            {
                line = (Line)line.Clone();
                float stepFraction = Math.Min((float)(targetAlpha - reachedAlpha) / (255 - reachedAlpha), 0.75f);
                int stepAlpha = (int)(stepFraction * 255);

                if (line.Sections.Count > 1)
                {
                    line.Sections[0].Text = line.Text;
                    line.Sections.RemoveRange(1, line.Sections.Count - 1);
                }
                line.Sections[0].ForeColor = ColorUtil.ChangeColorAlpha(line.Sections[0].ForeColor, 0);
                line.Sections[0].ShadowTypes = ShadowType.None;
                line.Sections[0].BackColor = ColorUtil.ChangeColorAlpha(line.Sections[0].BackColor, stepAlpha);

                reachedAlpha += (int)((255 - reachedAlpha) * stepFraction);
                Lines.Insert(lineIndex, line);
                numReplacementLines++;
            }
            return numReplacementLines;
        }

        /// <summary>
        /// YTSubConverter feature: one line with multiple shadows
        /// </summary>
        private int ExpandLineForMultiShadows(int lineIndex)
        {
            Line line = Lines[lineIndex];
            if (line.Sections.Count == 0)
                return 1;

            List<ShadowType> shadowTypes = new List<ShadowType>();
            foreach (ShadowType shadowType in new[] { ShadowType.SoftShadow, ShadowType.HardShadow, ShadowType.Glow })
            {
                if ((line.Sections[0].ShadowTypes & shadowType) != 0)
                    shadowTypes.Add(shadowType);
            }

            if (shadowTypes.Count <= 1)
                return 1;

            Lines.RemoveAt(lineIndex);
            for (int i = 0; i < shadowTypes.Count; i++)
            {
                Line shadowLine = (Line)line.Clone();

                if (i < shadowTypes.Count - 1)
                {
                    shadowLine.Sections.Clear();

                    Section section =
                        new Section(line.Text)
                        {
                            ForeColor = ColorUtil.ChangeColorAlpha(line.Sections[0].ForeColor, 0),
                            BackColor = i == 0 ? line.Sections[0].BackColor : Color.Empty,
                            ShadowTypes = shadowTypes[i],
                            ShadowColor = line.Sections[0].ShadowColor
                        };
                    shadowLine.Sections.Add(section);
                }
                else
                {
                    foreach (Section section in shadowLine.Sections)
                    {
                        section.BackColor = Color.Empty;
                        section.ShadowTypes = shadowTypes[i];
                    }
                }

                Lines.Insert(lineIndex + i, shadowLine);
            }

            return shadowTypes.Count;
        }

        /// <summary>
        /// This method handles two problems: broken multicolored text, and lack of custom background colors on mobile.
        /// 
        /// YouTube used to support multiple different styles within a line, but in the beginning of November 2018,
        /// they broke it. Attempting to use multiple styles in a line now results in partial or complete loss of formatting.
        /// 
        /// We can at least restore support for multiple colors by overlaying multiple single-color lines as follows:
        ///     Layer 1 (#0000FF,    background): "Red Green Blue"
        ///     Layer 2 (#00FF00, no background): "Red Green"
        ///     Layer 3 (#FF0000, no background): "Red"
        /// 
        /// This looks good on PC, but because subtitles on mobile always have a padded background, the "G" and "B" would
        /// be partially covered by the background of layers 3 and 2 respectively. For this reason, we add another
        /// layer which contains the whole text (to prevent clipping) with one color (because we can't use multiple
        /// sections no matter what) and 0% opacity (so it only gets displayed on mobile).
        ///
        /// The mobile-only layer is also handy to address the second problem. Even in single-colored lines,
        /// we can't change the background color on mobile, so dark text on a bright background will be
        /// displayed as dark text on a black background there - unreadable. By adding an invisible layer,
        /// we can make the text bright on mobile only: not the intended color scheme, but at least readable.
        ///
        /// Note that the extra layer doesn't work on iOS - it doesn't get displayed there.
        /// </summary>
        private int ExpandLineForColoring(int lineIndex)
        {
            Line originalLine = Lines[lineIndex];
            originalLine.Sections.RemoveAll(s => s.Text.Length == 0);
            if (originalLine.Sections.Count == 1 && !ColorUtil.IsDark(originalLine.Sections[0].ForeColor))
                return 1;

            AnchorPoint anchorPoint = originalLine.AnchorPoint ?? AnchorPoint.BottomCenter;
            if (AnchorPointUtil.IsMiddleAligned(anchorPoint) && originalLine.Sections.Any(s => s.Text.Contains("\r\n")))
                throw new NotSupportedException(string.Format(Resources.VerticallyCenteredLinesWithMultipleStylesCantHaveLineBreaks0, originalLine.Text));

            Lines.RemoveAt(lineIndex);
            int numReplacementLines = 0;

            // Duplicate the line for each section
            List<List<Section>> subLines = GetSubLines(originalLine);
            int startSubLineIdx, endSubLineIdx, subLineIdxStep;
            if (AnchorPointUtil.IsTopAligned(anchorPoint))
            {
                startSubLineIdx = subLines.Count - 1;
                endSubLineIdx = -1;
                subLineIdxStep = -1;
            }
            else
            {
                startSubLineIdx = 0;
                endSubLineIdx = subLines.Count;
                subLineIdxStep = 1;
            }

            Section prevSection = null;
            for (int subLineIdx = startSubLineIdx; subLineIdx != endSubLineIdx; subLineIdx += subLineIdxStep)
            {
                List<Line> newLines = CreateColoredLinesFromSubLine(originalLine, subLines, subLineIdx, ref prevSection);
                foreach (Line newLine in newLines)
                {
                    Lines.Insert(lineIndex + numReplacementLines, newLine);
                    numReplacementLines++;
                }

                List<Section> subLine = subLines[subLineIdx];
                if (subLine.Count > 1 || subLine.Any(s => ColorUtil.IsDark(s.ForeColor)))
                {
                    Line mobileLine = CreateMobileLineFromSubLine(originalLine, subLines, subLineIdx);
                    Lines.Insert(lineIndex + numReplacementLines, mobileLine);
                    numReplacementLines++;
                }
            }

            return numReplacementLines;
        }

        private List<Line> CreateColoredLinesFromSubLine(Line originalLine, List<List<Section>> subLines, int subLineIdx, ref Section prevSection)
        {
            List<Line> newLines = new List<Line>();
            List<Section> subLineSections = subLines[subLineIdx];

            // Sections with certain special characters may get rendered a few pixels lower than sections without,
            // meaning that if a line has a mix of these, the multicolor workaround will result in visual artifacts.
            // For example, if you have the text "itsudemo i love you❤" where the text is white and the
            // heart is pink, the multicolor workaround will generate a pink "itsudemo i love you❤"
            // overlapped by a white "itsudemo i love you", but because the pink subtitle is displayed
            // slightly lower than the white one, the white text ends up having pink pixels at the bottom.
            // The workaround is to give *all* subtitles a character that triggers the pushdown.
            // (We use a full-width space which triggers it on both Firefox and Chrome)
            bool needPushdownWorkaround = originalLine.Sections.SelectMany(s => s.Text).Any(c => c < 0x100) &&
                                          originalLine.Sections.SelectMany(s => s.Text).Any(c => c > 0x100);

            for (int numSections = subLineSections.Count; numSections >= 1; numSections--)
            {
                int sectionIndex;
                Section newSection;
                if (AnchorPointUtil.IsLeftAligned(originalLine.AnchorPoint ?? AnchorPoint.BottomCenter))
                {
                    sectionIndex = numSections - 1;
                    newSection = (Section)subLineSections[sectionIndex].Clone();
                    newSection.Text = string.Join("", subLineSections.Take(numSections).Select(s => s.Text));
                    if (needPushdownWorkaround)
                        newSection.Text += "　";
                }
                else
                {
                    sectionIndex = subLineSections.Count - numSections;
                    newSection = (Section)subLineSections[sectionIndex].Clone();
                    newSection.Text = string.Join("", subLineSections.Skip(sectionIndex).Take(numSections).Select(s => s.Text));
                    if (needPushdownWorkaround)
                        newSection.Text = "　" + newSection.Text;
                }

                if (prevSection != null)
                {
                    // If the previous layer already gave us the background we need, we don't need to render it again
                    if (newSection.BackColor == prevSection.BackColor)
                        newSection.BackColor = ColorUtil.ChangeColorAlpha(newSection.BackColor, 0);

                    // Same for the shadow (disabled because additional shadows help cover up the visual artifacts from overlapping text)
                    //if (newSection.ShadowType == prevSection.ShadowType && newSection.ShadowColor == prevSection.ShadowColor)
                    //    newSection.ShadowType = ShadowType.None;
                }

                PadSectionForColoring(newSection, originalLine, subLines, subLineIdx);

                Line newLine = (Line)originalLine.Clone();
                newLine.Sections.Clear();
                newLine.Sections.Add(newSection);
                newLines.Add(newLine);

                prevSection = subLineSections[sectionIndex];
            }
            return newLines;
        }

        private Line CreateMobileLineFromSubLine(Line originalLine, List<List<Section>> subLines, int subLineIdx)
        {
            List<Section> subLine = subLines[subLineIdx];
            Section newSection = (Section)subLine[0].Clone();
            newSection.Text = string.Join("", subLine.Select(s => s.Text));
            if (ColorUtil.IsDark(newSection.ForeColor))
                newSection.ForeColor = ColorUtil.Brighten(newSection.ForeColor);

            newSection.BackColor = ColorUtil.ChangeColorAlpha(newSection.BackColor, 0);
            newSection.ForeColor = ColorUtil.ChangeColorAlpha(newSection.ForeColor, 0);
            newSection.ShadowTypes = ShadowType.None; // Edges don't follow foreground opacity, so explicitly disable

            PadSectionForColoring(newSection, originalLine, subLines, subLineIdx);

            Line newLine = (Line)originalLine.Clone();
            newLine.Sections.Clear();
            newLine.Sections.Add(newSection);
            return newLine;
        }

        private void PadSectionForColoring(Section newSection, Line originalLine, List<List<Section>> subLines, int subLineIdx)
        {
            bool topAligned = AnchorPointUtil.IsTopAligned(originalLine.AnchorPoint ?? AnchorPoint.BottomCenter);
            if (topAligned)
            {
                if (subLineIdx > 0)
                {
                    IEnumerable<string> remainingSubLines = subLines.Take(subLineIdx).Select(l => string.Join("", l.Select(s => s.Text)));
                    newSection.Text = string.Join("\r\n", remainingSubLines) + "\r\n" + newSection.Text;
                }
            }
            else
            {
                if (subLineIdx < subLines.Count - 1)
                {
                    IEnumerable<string> remainingSubLines = subLines.Skip(subLineIdx + 1).Select(l => string.Join("", l.Select(s => s.Text)));
                    newSection.Text = newSection.Text + "\r\n" + string.Join("\r\n", remainingSubLines);
                }
            }
        }

        private void WriteHead(XmlWriter writer, List<Line> positions, List<Section> pens)
        {
            writer.WriteStartElement("head");

            for (int i = 0; i < positions.Count; i++)
            {
                WriteWindowPosition(writer, i, positions[i]);
            }

            for (int i = 0; i < 3; i++)
            {
                WriteWindowStyle(writer, i, i);
            }

            for (int i = 0; i < pens.Count; i++)
            {
                WritePen(writer, i, pens[i]);
            }

            writer.WriteEndElement();
        }

        private void WriteWindowPosition(XmlWriter writer, int positionId, Line position)
        {
            AnchorPoint anchorPoint = position.AnchorPoint ?? AnchorPoint.BottomCenter;
            int anchorPointId = GetAnchorPointId(anchorPoint);

            PointF pixelCoords = position.Position ?? GetDefaultPosition(anchorPoint);
            PointF percentCoords = new PointF(pixelCoords.X / VideoDimensions.Width * 100, pixelCoords.Y / VideoDimensions.Height * 100);

            writer.WriteStartElement("wp");
            writer.WriteAttributeString("id", positionId.ToString());
            writer.WriteAttributeString("ap", anchorPointId.ToString());
            writer.WriteAttributeString("ah", ((int)CounteractYouTubePositionScaling(percentCoords.X)).ToString());
            writer.WriteAttributeString("av", ((int)CounteractYouTubePositionScaling(percentCoords.Y)).ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// YouTube decided to be helpful by moving your subtitles slightly towards the center so they'll never sit at the video's edge.
        /// However, it doesn't just impose a cap on each coordinate - it moves your sub regardless of where it is. You doesn't just
        /// get your X = 0% changed to a 2%, but also your 10% to an 11.6%, for example.
        /// We counteract this cleverness so our subs actually get displayed where we said they should be.
        /// </summary>
        private static float CounteractYouTubePositionScaling(float percentage)
        {
            percentage = (percentage - 2) / 0.96f;
            percentage = Math.Max(percentage, 0);
            percentage = Math.Min(percentage, 100);
            return percentage;
        }

        private void WriteWindowStyle(XmlWriter writer, int styleId, int justify)
        {
            writer.WriteStartElement("ws");
            writer.WriteAttributeString("id", styleId.ToString());
            writer.WriteAttributeString("ju", justify.ToString());
            writer.WriteEndElement();
        }

        private void WritePen(XmlWriter writer, int penId, Section format)
        {
            writer.WriteStartElement("pen");
            writer.WriteAttributeString("id", penId.ToString());

            int fontId = GetFontId(format.Font);
            if (fontId != 0)
                writer.WriteAttributeString("fs", fontId.ToString());

            if (format.Bold)
                writer.WriteAttributeString("b", "1");

            if (format.Italic)
                writer.WriteAttributeString("i", "1");

            if (format.Underline)
                writer.WriteAttributeString("u", "1");

            Color foreColor = IsWhiteOrEmpty(format.ForeColor) ? Color.FromArgb(format.ForeColor.A, 254, 254, 254) : format.ForeColor;
            writer.WriteAttributeString("fc", ColorUtil.ToHtml(foreColor));
            writer.WriteAttributeString("fo", foreColor.A.ToString());

            if (!format.BackColor.IsEmpty)
            {
                writer.WriteAttributeString("bc", ColorUtil.ToHtml(format.BackColor));
                writer.WriteAttributeString("bo", format.BackColor.A.ToString());
            }
            else
            {
                writer.WriteAttributeString("bo", "0");
            }

            if (format.ShadowTypes != ShadowType.None && format.ShadowColor.A > 0)
            {
                writer.WriteAttributeString("et", GetEdgeType(format.ShadowTypes).ToString());
                writer.WriteAttributeString("ec", ColorUtil.ToHtml(format.ShadowColor));
            }

            writer.WriteEndElement();
        }

        private void WriteBody(XmlWriter writer, Dictionary<Line, int> positionIds, Dictionary<Section, int> penIds)
        {
            writer.WriteStartElement("body");
            foreach (Line line in Lines)
            {
                WriteLine(writer, line, positionIds, penIds);
            }
            writer.WriteEndElement();
        }

        private void WriteLine(XmlWriter writer, Line line, Dictionary<Line, int> positionIds, Dictionary<Section, int> penIds)
        {
            if (line.Sections.Count == 0)
                return;

            // Compensate for the subtitle delay (YouTube displaying the subtitle too late) by moving the start time up.
            // Also, because the Android app does not respect the positioning of (and sometimes does not display)
            // subtitles that start at 0ms, use 1ms in that case.
            int lineStartMs = (int)(line.Start - TimeBase).TotalMilliseconds;
            lineStartMs = Math.Max(lineStartMs - SubtitleDelayMs, 1);

            // Both the line's start and end time need to be compensated for YouTube's delay. Normally it's enough to
            // move up the start time and keep the duration constant, but if we can't move the start time far enough,
            // we also need to lessen the duration.
            int lineDurationMs = (int)(line.End - line.Start).TotalMilliseconds;
            if (lineStartMs < SubtitleDelayMs)
                lineDurationMs -= SubtitleDelayMs - lineStartMs + 1;

            if (lineDurationMs <= 0)
                return;

            writer.WriteStartElement("p");
            writer.WriteAttributeString("t", lineStartMs.ToString());
            writer.WriteAttributeString("d", lineDurationMs.ToString());
            if (line.Sections.Count == 1)
                writer.WriteAttributeString("p", penIds[line.Sections[0]].ToString());

            writer.WriteAttributeString("wp", positionIds[line].ToString());
            writer.WriteAttributeString("ws", GetWindowStyleId(line.AnchorPoint ?? AnchorPoint.BottomCenter).ToString());

            if (line.Sections.Count == 1)
            {
                writer.WriteValue(line.Sections[0].Text);
            }
            else
            {
                throw new NotSupportedException($"YouTube's support for multiple sections in a line is currently broken on PC ({line.Text})");
                
                foreach (Section section in line.Sections)
                {
                    WriteSection(writer, section, penIds);
                }
            }

            writer.WriteEndElement();
        }

        private void WriteSection(XmlWriter writer, Section section, Dictionary<Section, int> penIds)
        {
            writer.WriteStartElement("s");
            writer.WriteAttributeString("p", penIds[section].ToString());
            writer.WriteValue(section.Text);
            writer.WriteEndElement();
        }

        private static void ExtractAttributes<T>(
            IEnumerable<T> objects,
            IEqualityComparer<T> comparer,
            out Dictionary<T, int> mappings,
            out List<T> attributes
        )
        {
            mappings = new Dictionary<T, int>();
            attributes = new List<T>();
            foreach (T attr in objects)
            {
                int index = attributes.IndexOf(attr, comparer);
                if (index < 0)
                {
                    index = attributes.Count;
                    attributes.Add(attr);
                }
                mappings[attr] = index;
            }
        }

        private static int GetAnchorPointId(AnchorPoint anchorPoint)
        {
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                    return 0;

                case AnchorPoint.TopCenter:
                    return 1;

                case AnchorPoint.TopRight:
                    return 2;

                case AnchorPoint.MiddleLeft:
                    return 3;

                case AnchorPoint.Center:
                    return 4;

                case AnchorPoint.MiddleRight:
                    return 5;

                case AnchorPoint.BottomLeft:
                    return 6;

                case AnchorPoint.BottomCenter:
                    return 7;

                case AnchorPoint.BottomRight:
                    return 8;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private PointF GetDefaultPosition(AnchorPoint anchorPoint)
        {
            float left = VideoDimensions.Width * 0.02f;
            float center = VideoDimensions.Width / 2.0f;
            float right = VideoDimensions.Width * 0.98f;

            float top = VideoDimensions.Height * 0.02f;
            float middle = VideoDimensions.Height / 2.0f;
            float bottom = VideoDimensions.Height * 0.98f;
            
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                    return new PointF(left, top);

                case AnchorPoint.TopCenter:
                    return new PointF(center, top);

                case AnchorPoint.TopRight:
                    return new PointF(right, top);

                case AnchorPoint.MiddleLeft:
                    return new PointF(left, middle);

                case AnchorPoint.Center:
                    return new PointF(center, middle);

                case AnchorPoint.MiddleRight:
                    return new PointF(right, middle);

                case AnchorPoint.BottomLeft:
                    return new PointF(left, bottom);

                case AnchorPoint.BottomCenter:
                    return new PointF(center, bottom);

                case AnchorPoint.BottomRight:
                    return new PointF(right, bottom);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetWindowStyleId(AnchorPoint anchorPoint)
        {
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                case AnchorPoint.MiddleLeft:
                case AnchorPoint.BottomLeft:
                    return 0;

                case AnchorPoint.TopCenter:
                case AnchorPoint.Center:
                case AnchorPoint.BottomCenter:
                    return 2;

                case AnchorPoint.TopRight:
                case AnchorPoint.MiddleRight:
                case AnchorPoint.BottomRight:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetEdgeType(ShadowType type)
        {
            switch (type)
            {
                case ShadowType.HardShadow:
                    return 1;

                case ShadowType.SoftShadow:
                    return 4;

                case ShadowType.Glow:
                    return 3;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetFontId(string font)
        {
            switch (font)
            {
                case "Courier New":
                case "Courier":
                case "Nimbus Mono L":
                case "Cutive Mono":
                    return 1;

                case "Times New Roman":
                case "Times":
                case "Georgia":
                case "Cambria":
                case "PT Serif Caption":
                    return 2;

                case "Deja Vu Sans Mono":
                case "Lucida Console":
                case "Monaco":
                case "Consolas":
                case "PT Mono":
                    return 3;

                case "Comic Sans MS":
                case "Impact":
                case "Handlee":
                    return 5;

                case "Monotype Corsiva":
                case "URW Chancery L":
                case "Apple Chancery":
                case "Dancing Script":
                    return 6;

                case "Carrois Gothic SC":
                    return 7;

                default:
                    return 0;
            }
        }

        private static bool IsWhiteOrEmpty(Color color)
        {
            if (color.IsEmpty)
                return true;

            return color.R == 255 &&
                   color.G == 255 &&
                   color.B == 255;
        }

        private struct LinePositionComparer : IEqualityComparer<Line>
        {
            public bool Equals(Line x, Line y)
            {
                return x.AnchorPoint == y.AnchorPoint &&
                       x.Position == y.Position;
            }

            public int GetHashCode(Line line)
            {
                return (line.AnchorPoint?.GetHashCode() ?? 0) ^
                       (line.Position?.GetHashCode() ?? 0);
            }
        }
    }
}
