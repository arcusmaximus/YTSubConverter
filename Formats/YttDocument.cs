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
        }

        public override void Save(string filePath)
        {
            CloseGaps();
            ApplyEnhancements();

            ExtractAttributes(
                Lines,
                new LinePositionComparer(),
                out Dictionary<Line, int> positionIds,
                out List<Line> positions
            );

            ExtractAttributes(
                Lines,
                new LineAlignmentComparer(),
                out Dictionary<Line, int> windowStyleIds,
                out List<Line> windowStyles
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

                WriteHead(writer, positions, windowStyles, pens);
                WriteBody(writer, positionIds, windowStyleIds, penIds);

                writer.WriteEndElement();
            }
        }

        private void ApplyEnhancements()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                HardenSpaces(i);
                AddRubySafetySection(i);
                SplitMultiBackgroundLine(i);
                i += ExpandLineForMultiShadows(i) - 1;
                i += ExpandLineForDarkText(i) - 1;
            }
            AddItalicPrefetch();
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
        /// To ensure the first section of a multisection line doesn't lose its "p" attribute, we add a zero-width space
        /// after it (<see cref="WriteLine"/>). However, this workaround also breaks any ruby groups that
        /// are right at the start of the line (because the separator is spliced into the group). To prevent this,
        /// we prepend a dummy section so the separator will appear before the ruby group inside of inside it.
        /// </summary>
        private void AddRubySafetySection(int lineIndex)
        {
            Line line = Lines[lineIndex];
            if (line.Sections.Count == 0 || line.Sections[0].RubyPart != RubyPart.Text)
                return;

            Section dummySection = (Section)line.Sections[0].Clone();
            dummySection.Text = "x";
            dummySection.RubyPart = RubyPart.None;
            line.Sections.Insert(0, dummySection);
        }

        /// <summary>
        /// Even after taking months to fix a regression that completely broke the use of multiple formats
        /// in one subtitle, YouTube still hasn't got the Javascript code down. While changing the background
        /// color in the middle of a line now works again, changing it before or after a line break results
        /// in the rounded corners disappearing on one side. It doesn't look pretty.
        /// The only workaround seems to be to split the subtitle in two.
        /// </summary>
        private void SplitMultiBackgroundLine(int lineIndex)
        {
            Line line = Lines[lineIndex];
            if (AnchorPointUtil.IsMiddleAligned(line.AnchorPoint))
                return;

            int numLineBreaks = line.Sections.SelectMany(s => s.Text).Count(c => c == '\n');
            if (numLineBreaks != 1)
                return;

            int secondSubLineStartSectionIdx = -1;
            for (int i = 1; i < line.Sections.Count; i++)
            {
                Section prevSection = line.Sections[i - 1];
                Section section = line.Sections[i];
                if (prevSection.BackColor != section.BackColor && (prevSection.Text.EndsWith("\r\n") || section.Text.StartsWith("\r\n")))
                {
                    secondSubLineStartSectionIdx = i;
                    break;
                }
            }

            if (secondSubLineStartSectionIdx < 0)
                return;

            Line secondLine = (Line)line.Clone();
            line.Sections.RemoveRange(secondSubLineStartSectionIdx, line.Sections.Count - secondSubLineStartSectionIdx);
            line.Sections[secondSubLineStartSectionIdx - 1].Text = line.Sections[secondSubLineStartSectionIdx - 1].Text.Replace("\r\n", "");
            secondLine.Sections.RemoveRange(0, secondSubLineStartSectionIdx);
            secondLine.Sections[0].Text = secondLine.Sections[0].Text.Replace("\r\n", "");

            PointF position = line.Position ?? GetDefaultPosition(line.AnchorPoint);
            if (AnchorPointUtil.IsTopAligned(line.AnchorPoint))
            {
                position.Y += VideoDimensions.Height * 0.05f;
                line.AnchorPoint = AnchorPointUtil.GetVerticalOpposite(line.AnchorPoint);
            }
            else
            {
                position.Y -= VideoDimensions.Height * 0.05f;
                secondLine.AnchorPoint = AnchorPointUtil.GetVerticalOpposite(secondLine.AnchorPoint);
            }

            line.Position = position;
            secondLine.Position = position;
            Lines.Insert(lineIndex + 1, secondLine);
        }

        /// <summary>
        /// YTSubConverter supports multiple shadow types (and colors) on one subtitle by duplicating it as necessary
        /// </summary>
        private int ExpandLineForMultiShadows(int lineIndex)
        {
            Line line = Lines[lineIndex];
            int maxNumShadows = line.Sections.Max(s => s.ShadowColors.Count);
            if (maxNumShadows <= 1)
                return 1;

            List<List<ShadowType>> lineLayerShadowTypes = new List<List<ShadowType>>();
            ShadowType[] orderedShadowTypes = { ShadowType.SoftShadow, ShadowType.HardShadow, ShadowType.Bevel, ShadowType.Glow };
            foreach (Section section in line.Sections)
            {
                List<ShadowType> sectionLayerShadowTypes = new List<ShadowType>();
                foreach (ShadowType shadowType in orderedShadowTypes)
                {
                    if (section.ShadowColors.ContainsKey(shadowType))
                        sectionLayerShadowTypes.Add(shadowType);
                }
                lineLayerShadowTypes.Add(sectionLayerShadowTypes);
            }

            Lines.RemoveAt(lineIndex);

            for (int layerIdx = 0; layerIdx < maxNumShadows; layerIdx++)
            {
                Line shadowLine = (Line)line.Clone();
                for (int sectionIdx = 0; sectionIdx < shadowLine.Sections.Count; sectionIdx++)
                {
                    Section section = shadowLine.Sections[sectionIdx];
                    List<ShadowType> sectionLayerShadowTypes = lineLayerShadowTypes[sectionIdx];

                    if (layerIdx > 0)
                        section.BackColor = ColorUtil.ChangeColorAlpha(section.BackColor, 0);

                    if (layerIdx < maxNumShadows - 1)
                        section.ForeColor = ColorUtil.ChangeColorAlpha(section.ForeColor, 0);

                    if (layerIdx < sectionLayerShadowTypes.Count)
                        section.ShadowColors.RemoveAll(t => t != sectionLayerShadowTypes[layerIdx]);
                    else
                        section.ShadowColors.Clear();
                }
                Lines.Insert(lineIndex + layerIdx, shadowLine);
            }

            return maxNumShadows;
        }

        /// <summary>
        /// The mobile apps have an unchangeable black background, meaning dark text is unreadable there.
        /// As a workaround, we overlap the dark subtitle with an invisible bright one: this way, we
        /// get the custom background and dark text on PC, and a black background and bright text
        /// on Android (because the Android app doesn't support transparency).
        /// Sadly, this trick doesn't work for iOS: that one supports (only) text transparency,
        /// meaning our bright yet invisible subtitle doesn't show up there.
        /// </summary>
        private int ExpandLineForDarkText(int lineIdx)
        {
            Line line = Lines[lineIdx];
            if (!line.Sections.Any(s => ColorUtil.IsDark(s.ForeColor)))
                return 1;

            Line brightLine = (Line)line.Clone();
            foreach (Section section in brightLine.Sections)
            {
                if (ColorUtil.IsDark(section.ForeColor))
                    section.ForeColor = ColorUtil.Brighten(section.ForeColor);

                section.ForeColor = ColorUtil.ChangeColorAlpha(section.ForeColor, 0);
                section.BackColor = ColorUtil.ChangeColorAlpha(section.BackColor, 0);
                section.ShadowColors.Clear();
            }

            Lines.Insert(lineIdx + 1, brightLine);
            return 2;
        }

        /// <summary>
        /// On PC, the first piece of italic text shows up with a noticeable delay: the background appears as usual,
        /// but for a fraction of a second after that, the text is either shown as non-italic or not shown at all.
        /// To work around this, we add an invisible italic subtitle at the start to make YouTube eagerly load
        /// whatever it normally loads lazily.
        /// </summary>
        private void AddItalicPrefetch()
        {
            Line italicLine =
                new Line(TimeBase.AddMilliseconds(SubtitleDelayMs), TimeBase.AddMilliseconds(SubtitleDelayMs + 100))
                {
                    Position = new PointF(0, 0),
                    AnchorPoint = AnchorPoint.BottomRight
                };
            Section section =
                new Section("\x200B")
                {
                    ForeColor = Color.FromArgb(1, 0, 0, 0),
                    BackColor = Color.Empty,
                    Italic = true
                };
            italicLine.Sections.Add(section);
            Lines.Add(italicLine);
        }

        private void WriteHead(XmlWriter writer, List<Line> positions, List<Line> windowStyles, List<Section> pens)
        {
            writer.WriteStartElement("head");

            for (int i = 0; i < positions.Count; i++)
            {
                WriteWindowPosition(writer, i, positions[i]);
            }

            for (int i = 0; i < windowStyles.Count; i++)
            {
                WriteWindowStyle(writer, i, windowStyles[i]);
            }

            for (int i = 0; i < pens.Count; i++)
            {
                WritePen(writer, i, pens[i]);
            }

            writer.WriteEndElement();
        }

        private void WriteWindowPosition(XmlWriter writer, int positionId, Line position)
        {
            PointF pixelCoords = position.Position ?? GetDefaultPosition(position.AnchorPoint);
            PointF percentCoords = new PointF(pixelCoords.X / VideoDimensions.Width * 100, pixelCoords.Y / VideoDimensions.Height * 100);

            writer.WriteStartElement("wp");
            writer.WriteAttributeString("id", positionId.ToString());
            writer.WriteAttributeString("ap", GetAnchorPointId(position.AnchorPoint).ToString());
            writer.WriteAttributeString("ah", ((int)CounteractYouTubePositionScaling(percentCoords.X)).ToString());
            writer.WriteAttributeString("av", ((int)CounteractYouTubePositionScaling(percentCoords.Y)).ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// YouTube decided to be helpful by moving your subtitles slightly towards the center so they'll never sit at the video's edge.
        /// However, it doesn't just impose a cap on each coordinate - it moves your sub regardless of where it is. For example,
        /// you doesn't just get your X = 0% changed to a 2%, but also your 10% to an 11.6%.
        /// We counteract this cleverness so our subs actually get displayed where we said they should be.
        /// (Or at least as close as possible because the server doesn't allow floating point coordinates for whatever reason)
        /// </summary>
        private static float CounteractYouTubePositionScaling(float percentage)
        {
            percentage = (percentage - 2) / 0.96f;
            percentage = Math.Max(percentage, 0);
            percentage = Math.Min(percentage, 100);
            return percentage;
        }

        private void WriteWindowStyle(XmlWriter writer, int styleId, Line style)
        {
            writer.WriteStartElement("ws");
            writer.WriteAttributeString("id", styleId.ToString());
            writer.WriteAttributeString("ju", GetJustificationId(style.AnchorPoint).ToString());
            writer.WriteAttributeString("pd", GetTextDirectionId(style.VerticalTextType).ToString());
            writer.WriteAttributeString("sd", IsLineFlowInverted(style.VerticalTextType) ? "1" : "0");
            writer.WriteEndElement();
        }

        private void WritePen(XmlWriter writer, int penId, Section format)
        {
            writer.WriteStartElement("pen");
            writer.WriteAttributeString("id", penId.ToString());

            int fontStyleId = GetFontStyleId(format.Font);
            if (fontStyleId != 0)
                writer.WriteAttributeString("fs", fontStyleId.ToString());

            if (format.Scale != 1)
            {
                // Similar to window positions, YouTube refuses to simply take the specified size percentage and apply it.
                // Instead, they do actualScale = 1 + (specifiedScale - 1) / 4, meaning that specifying a 200% size
                // results in only 125% and that you can't go lower than an actual scale of 75% (specifiedScale = 0).
                // Maybe they do this to allow for more granularity? But then why not simply allow floating point numbers? Who knows...
                float yttScale = Math.Max(1 + (format.Scale - 1) * 4, 0);
                writer.WriteAttributeString("sz", ((int)(yttScale * 100)).ToString());
            }

            if (format.Offset != OffsetType.Regular)
                writer.WriteAttributeString("of", GetOffsetTypeId(format.Offset).ToString());

            if (format.Bold)
                writer.WriteAttributeString("b", "1");

            if (format.Italic)
                writer.WriteAttributeString("i", "1");

            if (format.Underline)
                writer.WriteAttributeString("u", "1");

            // Lots of pen attributes get removed if the foreground color is white -> use #FEFEFE instead
            Color foreColor = IsWhiteOrEmpty(format.ForeColor) ? Color.FromArgb(format.ForeColor.A, 254, 254, 254) : format.ForeColor;
            writer.WriteAttributeString("fc", ColorUtil.ToHtml(foreColor));
            writer.WriteAttributeString("fo", foreColor.A.ToString());

            if (!format.BackColor.IsEmpty)
            {
                // The "bo" attribute gets removed if it's 255, even though this results in the default opacity of 75% (not 100%) on PC -> use 254 instead
                writer.WriteAttributeString("bc", ColorUtil.ToHtml(format.BackColor));
                writer.WriteAttributeString("bo", Math.Min((int)format.BackColor.A, 254).ToString());
            }
            else
            {
                writer.WriteAttributeString("bo", "0");
            }

            if (format.ShadowColors.Count > 0)
            {
                if (format.ShadowColors.Count > 1)
                    throw new NotSupportedException("YTT lines must be reduced to one shadow color before saving");

                KeyValuePair<ShadowType, Color> shadowColor = format.ShadowColors.First();
                if (shadowColor.Value.A > 0)
                {
                    writer.WriteAttributeString("et", GetEdgeTypeId(shadowColor.Key).ToString());

                    // YouTube's handling of shadow transparency is inconsistent: if you specify an "ec" attribute,
                    // the shadow is fully opaque, but if you don't (resulting in a default color of #222222),
                    // it follows the foreground transparency. Because of this, we only write the "ec" attribute
                    // (and lose transparency support) if we have to.
                    if (shadowColor.Value.R != 0x22 ||
                        shadowColor.Value.G != 0x22 ||
                        shadowColor.Value.B != 0x22 ||
                        shadowColor.Value.A != foreColor.A)
                    {
                        writer.WriteAttributeString("ec", ColorUtil.ToHtml(shadowColor.Value));
                    }
                }
            }

            if (format.RubyPart != RubyPart.None)
                writer.WriteAttributeString("rb", GetRubyPartId(format.RubyPart).ToString());

            if (format.Packed)
                writer.WriteAttributeString("hg", "1");

            writer.WriteEndElement();
        }

        private void WriteBody(XmlWriter writer, Dictionary<Line, int> positionIds, Dictionary<Line, int> windowStyleIds, Dictionary<Section, int> penIds)
        {
            writer.WriteStartElement("body");
            foreach (Line line in Lines)
            {
                WriteLine(writer, line, positionIds, windowStyleIds, penIds);
            }
            writer.WriteEndElement();
        }

        private void WriteLine(XmlWriter writer, Line line, Dictionary<Line, int> positionIds, Dictionary<Line, int> windowStyleIds, Dictionary<Section, int> penIds)
        {
            if (line.Sections.Count == 0)
                return;

            // Compensate for the subtitle delay (YouTube displaying the subtitle too late) by moving the start time up.
            int lineStartMs = (int)(line.Start - TimeBase).TotalMilliseconds - SubtitleDelayMs;
            int lineDurationMs = (int)(line.End - line.Start).TotalMilliseconds;

            // If subtracting the subtitle delay brought us into negative time (because the original starting time was less than
            // the delay), set the starting time to 1ms and reduce the duration instead.
            // (The reason for using 1ms is that the Android app does not respect the positioning of, and sometimes does not display,
            // subtitles that start at 0ms)
            if (lineStartMs <= 0)
            {
                lineDurationMs -= -lineStartMs + 1;
                lineStartMs = 1;
            }

            if (lineDurationMs <= 0)
                return;

            writer.WriteStartElement("p");
            writer.WriteAttributeString("t", lineStartMs.ToString());
            writer.WriteAttributeString("d", lineDurationMs.ToString());
            if (line.Sections.Count == 1)
                writer.WriteAttributeString("p", penIds[line.Sections[0]].ToString());

            writer.WriteAttributeString("wp", positionIds[line].ToString());
            writer.WriteAttributeString("ws", windowStyleIds[line].ToString());

            if (line.Sections.Count == 1)
            {
                writer.WriteValue(line.Sections[0].Text);
            }
            else
            {
                // The server will remove the "p" (pen ID) attribute of the first section unless the line has text that's not part of any section.
                // We use a Mongolian vowel separator after the first section to avoid visual impact. This is like a zero-width space,
                // except it doesn't increase the line height.
                bool multiSectionWorkaroundWritten = false;
                foreach (Section section in line.Sections)
                {
                    WriteSection(writer, section, penIds);
                    if (!multiSectionWorkaroundWritten)
                    {
                        writer.WriteCharEntity((char)0x200B);
                        multiSectionWorkaroundWritten = true;
                    }
                }
            }

            writer.WriteEndElement();
        }

        private void WriteSection(XmlWriter writer, Section section, Dictionary<Section, int> penIds)
        {
            writer.WriteStartElement("s");
            writer.WriteAttributeString("p", penIds[section].ToString());

            // Surround line breaks by Mongolian vowel separators just in case one of those breaks lies at
            // a section border (which would cause the rounded corners on that side to get cut off)
            writer.WriteValue(section.Text.Replace("\r\n", "\x200B\r\n\x200B"));

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

        private static int GetJustificationId(AnchorPoint anchorPoint)
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

        private static int GetTextDirectionId(VerticalTextType type)
        {
            switch (type)
            {
                case VerticalTextType.VerticalRtl:
                case VerticalTextType.VerticalLtr:
                    return 2;

                case VerticalTextType.RotatedLtr:
                case VerticalTextType.RotatedRtl:
                    return 3;

                default:
                    return 0;
            }
        }

        private static bool IsLineFlowInverted(VerticalTextType type)
        {
            return type == VerticalTextType.VerticalLtr ||
                   type == VerticalTextType.RotatedRtl;
        }

        private static int GetEdgeTypeId(ShadowType type)
        {
            switch (type)
            {
                case ShadowType.HardShadow:
                    return 1;

                case ShadowType.Bevel:
                    return 2;

                case ShadowType.Glow:
                    return 3;

                case ShadowType.SoftShadow:
                    return 4;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetOffsetTypeId(OffsetType type)
        {
            switch (type)
            {
                case OffsetType.Subscript:
                    return 0;

                case OffsetType.Regular:
                    return 1;

                case OffsetType.Superscript:
                    return 2;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetRubyPartId(RubyPart part)
        {
            switch (part)
            {
                case RubyPart.None:
                    return 0;

                case RubyPart.Text:
                    return 1;

                case RubyPart.Parenthesis:
                    return 2;

                case RubyPart.RubyAbove:
                    return 4;

                case RubyPart.RubyBelow:
                    return 5;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetFontStyleId(string font)
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
                return line.AnchorPoint.GetHashCode() ^
                       (line.Position?.GetHashCode() ?? 0);
            }
        }

        private struct LineAlignmentComparer : IEqualityComparer<Line>
        {
            public bool Equals(Line x, Line y)
            {
                return GetJustificationId(x.AnchorPoint) == GetJustificationId(y.AnchorPoint) &&
                       x.VerticalTextType == y.VerticalTextType;
            }

            public int GetHashCode(Line line)
            {
                return GetJustificationId(line.AnchorPoint) ^
                       line.VerticalTextType.GetHashCode();
            }
        }
    }
}
