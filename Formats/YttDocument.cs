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
            ApplyEnhancements();
            CloseGaps();

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

        private void ApplyEnhancements()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                HardenSpaces(i);
                i += ExpandLineForMultiShadows(i) - 1;
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

        private int ExpandLineForMultiShadows(int lineIndex)
        {
            Line line = Lines[lineIndex];
            if (line.Sections.Count == 0)
                return 1;

            HashSet<ShadowType> shadowTypes = new HashSet<ShadowType>();
            foreach (Section section in line.Sections)
            {
                shadowTypes.UnionWith(section.ShadowColors.Keys);
            }

            if (shadowTypes.Count <= 1)
                return 1;

            Lines.RemoveAt(lineIndex);

            int layerIdx = 0;
            foreach (ShadowType shadowType in new[] { ShadowType.SoftShadow, ShadowType.HardShadow, ShadowType.Glow })
            {
                if (!shadowTypes.Contains(shadowType))
                    continue;

                Line shadowLine = (Line)line.Clone();
                if (layerIdx < shadowTypes.Count - 1)
                {
                    shadowLine.Sections.Clear();
                    foreach (IGrouping<Color, Section> sectionGroup in line.Sections.GroupByContiguous(s => s.ShadowColors.GetOrDefault(shadowType)))
                    {
                        Section section = (Section)sectionGroup.First().Clone();
                        section.Text = string.Join("", sectionGroup.Select(s => s.Text));
                        section.ForeColor = ColorUtil.ChangeColorAlpha(section.ForeColor, 0);
                        section.BackColor = layerIdx == 0 ? section.BackColor : Color.Empty;
                        section.ShadowColors.RemoveAll(t => t != shadowType);
                        shadowLine.Sections.Add(section);
                    }
                }
                else
                {
                    foreach (Section section in shadowLine.Sections)
                    {
                        section.BackColor = Color.Empty;
                        section.ShadowColors.RemoveAll(t => t != shadowType);
                    }
                }

                Lines.Insert(lineIndex + layerIdx, shadowLine);
                layerIdx++;
            }

            return layerIdx;
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
                    writer.WriteAttributeString("et", GetEdgeType(shadowColor.Key).ToString());
                    writer.WriteAttributeString("ec", ColorUtil.ToHtml(shadowColor.Value));
                }
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
            writer.WriteAttributeString("ws", GetWindowStyleId(line.AnchorPoint ?? AnchorPoint.BottomCenter).ToString());

            if (line.Sections.Count == 1)
            {
                writer.WriteValue(line.Sections[0].Text);
            }
            else
            {
                // The server will remove the "p" (pen ID) attribute of the first section unless the line has text that's not part of any section.
                // We use a zero-width space after the first section to avoid visual impact.
                bool multiSectionWorkaroundWritten = false;
                foreach (Section section in line.Sections)
                {
                    WriteSection(writer, section, penIds);
                    if (!multiSectionWorkaroundWritten)
                    {
                        writer.WriteCharEntity((char)8203);
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
