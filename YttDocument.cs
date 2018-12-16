using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;

namespace Arc.YTSubConverter
{
    internal class YttDocument : SubtitleDocument
    {
        public YttDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public override void Save(string filePath)
        {
            ExtractAttributes(
                Lines.Where(l => (l.AnchorPoint ?? AnchorPoint.BottomCenter) != AnchorPoint.BottomCenter || l.Position != null),
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

            Color foreColor = IsWhiteOrEmpty(format.ForeColor) ? Color.FromArgb(254, 254, 254) : format.ForeColor;
            writer.WriteAttributeString("fc", ToHtmlColor(foreColor));
            writer.WriteAttributeString("fo", foreColor.A.ToString());

            if (format.BackColor != Color.Empty)
            {
                writer.WriteAttributeString("bc", ToHtmlColor(format.BackColor));
                writer.WriteAttributeString("bo", format.BackColor.A.ToString());
            }
            else
            {
                writer.WriteAttributeString("bo", "0");
            }

            if (format.ShadowType != ShadowType.None && format.ShadowColor.A > 0)
            {
                writer.WriteAttributeString("et", GetEdgeType(format.ShadowType).ToString());
                writer.WriteAttributeString("ec", ToHtmlColor(format.ShadowColor));
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

            writer.WriteStartElement("p");
            writer.WriteAttributeString("t", ((int)(line.Start - TimeBase).TotalMilliseconds).ToString());
            writer.WriteAttributeString("d", ((int)(line.End - line.Start).TotalMilliseconds).ToString());

            if ((line.AnchorPoint ?? AnchorPoint.BottomCenter) != AnchorPoint.BottomCenter || line.Position != null)
            {
                writer.WriteAttributeString("wp", positionIds[line].ToString());
                writer.WriteAttributeString("ws", GetWindowStyleId(line.AnchorPoint ?? AnchorPoint.BottomCenter).ToString());
            }

            if (line.Sections.Count == 1)
            {
                WriteSectionAttributesAndContent(writer, line.Sections[0], penIds);
            }
            else
            {
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
            WriteSectionAttributesAndContent(writer, section, penIds);
            writer.WriteEndElement();
        }

        private void WriteSectionAttributesAndContent(XmlWriter writer, Section section, Dictionary<Section, int> penIds)
        {
            writer.WriteAttributeString("p", penIds[section].ToString());
            if (section.TimeOffset != TimeSpan.Zero)
                writer.WriteAttributeString("t", ((int)section.TimeOffset.TotalMilliseconds).ToString());

            writer.WriteValue(section.Text);
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
                mappings.Add(attr, index);
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

        private static string ToHtmlColor(Color color)
        {
            return $"#{color.R:X02}{color.G:X02}{color.B:X02}";
        }

        private static bool IsWhiteOrEmpty(Color color)
        {
            if (color == Color.Empty)
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

            public int GetHashCode(Line obj)
            {
                throw new NotImplementedException();
            }
        }

        private struct SectionFormatComparer : IEqualityComparer<Section>
        {
            public bool Equals(Section x, Section y)
            {
                return x.Bold == y.Bold &&
                       x.Italic == y.Italic &&
                       x.Underline == y.Underline &&
                       x.Font == y.Font &&
                       x.ForeColor == y.ForeColor &&
                       x.BackColor == y.BackColor &&
                       x.ShadowColor == y.ShadowColor &&
                       x.ShadowType == y.ShadowType;
            }

            public int GetHashCode(Section obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
