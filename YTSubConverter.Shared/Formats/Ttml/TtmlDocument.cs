using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public class TtmlDocument : SubtitleDocument
    {
        internal static class Namespaces
        {
            public const string Xml = "http://www.w3.org/XML/1998/namespace";
            public const string Tt = "http://www.w3.org/ns/ttml";
            public const string Ttp = "http://www.w3.org/ns/ttml#parameter";
            public const string Tts = "http://www.w3.org/ns/ttml#styling";
        }

        public TtmlDocument()
        {
            CellResolution = new Size(32, 15);
            FrameRate = 30;
            SubFrameRate = 1;
            TickRate = 1;
            Styles = new Dictionary<string, TtmlStyle>();
            Regions = new Dictionary<string, TtmlRegion>();
        }

        public TtmlDocument(string filePath)
            : this()
        {
            using StreamReader reader = new StreamReader(filePath);
            Load(reader);
        }

        public TtmlDocument(Stream stream)
            : this()
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            Load(reader);
        }

        public TtmlDocument(TextReader reader)
            : this()
        {
            Load(reader);
        }

        public TtmlDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public Size CellResolution
        {
            get;
            private set;
        }

        public float FrameRate
        {
            get;
            private set;
        }

        public int SubFrameRate
        {
            get;
            private set;
        }

        public float TickRate
        {
            get;
            private set;
        }

        public TtmlStyle InitialStyle
        {
            get;
            private set;
        }

        public Dictionary<string, TtmlStyle> Styles
        {
            get;
        }

        public Dictionary<string, TtmlRegion> Regions
        {
            get;
        }

        public TtmlBody Body
        {
            get;
            private set;
        }

        private void Load(TextReader reader)
        {
            InitialStyle = TtmlStyle.CreateDefaultInitialStyle();

            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(reader);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xml", Namespaces.Xml);
            nsmgr.AddNamespace("tt", Namespaces.Tt);
            nsmgr.AddNamespace("ttp", Namespaces.Ttp);
            nsmgr.AddNamespace("tts", Namespaces.Tts);

            XmlElement ttElem = (XmlElement)doc.SelectSingleNode("/tt:tt", nsmgr);
            if (ttElem == null)
                throw new InvalidDataException("No <tt> element at root");

            ReadDocumentRoot(ttElem, nsmgr);

            XmlElement headElem = (XmlElement)ttElem.SelectSingleNode("tt:head", nsmgr);
            if (headElem != null)
                ReadHead(headElem, nsmgr);

            XmlElement bodyElem = (XmlElement)ttElem.SelectSingleNode("tt:body", nsmgr);
            if (bodyElem != null)
            {
                ReadBody(bodyElem, nsmgr);
                Lines.AddRange(ConvertTtmlToCommon());
            }
        }

        private void ReadDocumentRoot(XmlElement ttElem, XmlNamespaceManager nsmgr)
        {
            string tts = nsmgr.LookupNamespace("tts");
            string ttp = nsmgr.LookupNamespace("ttp");

            string extent = ttElem.GetAttribute("extent", tts);
            if (!string.IsNullOrEmpty(extent) &&
                TtmlSize.TryParse(extent, out TtmlSize parsedExtent) &&
                parsedExtent.Width.Unit == TtmlUnit.Pixels &&
                parsedExtent.Height.Unit == TtmlUnit.Pixels)
            {
                VideoDimensions = new Size((int)parsedExtent.Width.Value, (int)parsedExtent.Height.Value);
            }

            string cellResolution = ttElem.GetAttribute("cellResolution", ttp);
            if (!string.IsNullOrEmpty(cellResolution))
            {
                (int h, int v) = ParseIntPair(cellResolution);
                CellResolution = new Size(h, v);
            }

            string timeBase = ttElem.GetAttribute("timeBase", ttp);
            if (!string.IsNullOrEmpty(timeBase) && timeBase != "media")
                throw new NotSupportedException("ttp:timeBase other than \"media\" is not supported");

            string frameRate = ttElem.GetAttribute("frameRate", ttp);
            if (!string.IsNullOrEmpty(frameRate))
                FrameRate = int.Parse(frameRate);

            string frameRateMultiplier = ttElem.GetAttribute("frameRateMultiplier", ttp);
            if (!string.IsNullOrEmpty(frameRateMultiplier))
            {
                (int numerator, int denominator) = ParseIntPair(frameRateMultiplier);
                FrameRate = FrameRate * numerator / denominator;
            }

            string subframeRate = ttElem.GetAttribute("subFrameRate", ttp);
            if (!string.IsNullOrEmpty(subframeRate))
                SubFrameRate = int.Parse(subframeRate);

            string tickRate = ttElem.GetAttribute("tickRate", ttp);
            if (!string.IsNullOrEmpty(tickRate))
            {
                TickRate = int.Parse(tickRate);
            }
            else if (!string.IsNullOrEmpty(frameRate))
            {
                TickRate = FrameRate * SubFrameRate;
            }
        }

        private void ReadHead(XmlElement headElem, XmlNamespaceManager nsmgr)
        {
            XmlElement stylingElem = (XmlElement)headElem.SelectSingleNode("tt:styling", nsmgr);
            if (stylingElem != null)
                ReadStyles(stylingElem, nsmgr);

            XmlElement layoutElem = (XmlElement)headElem.SelectSingleNode("tt:layout", nsmgr);
            if (layoutElem != null)
                ReadRegions(layoutElem, nsmgr);
        }

        private void ReadStyles(XmlElement stylingElem, XmlNamespaceManager nsmgr)
        {
            XmlElement initialElem = (XmlElement)stylingElem.SelectSingleNode("tt:initial", nsmgr);
            if (initialElem != null)
                InitialStyle = new TtmlStyle(initialElem, nsmgr, InitialStyle) { IsInitial = true };

            Dictionary<TtmlStyle, string> baseStyleIds = new Dictionary<TtmlStyle, string>();
            foreach (XmlElement styleElem in stylingElem.SelectNodes("tt:style", nsmgr))
            {
                TtmlStyle style = new TtmlStyle(styleElem, nsmgr, InitialStyle);
                Styles.Add(style.Id, style);

                string baseStyleId = styleElem.GetAttribute("style");
                if (!string.IsNullOrEmpty(baseStyleId))
                    baseStyleIds.Add(style, baseStyleId);
            }

            foreach (KeyValuePair<TtmlStyle, string> baseStyleRef in baseStyleIds)
            {
                TtmlStyle style = baseStyleRef.Key;
                TtmlStyle baseStyle = Styles.GetOrDefault(baseStyleRef.Value);
                if (baseStyle != null)
                    style.BaseStyle = baseStyle;
            }
        }

        private void ReadRegions(XmlElement layoutElem, XmlNamespaceManager nsmgr)
        {
            foreach (XmlElement regionElem in layoutElem.SelectNodes("tt:region", nsmgr))
            {
                TtmlRegion region = new TtmlRegion(regionElem, nsmgr, this);
                Regions.Add(region.Id, region);
            }
        }

        private void ReadBody(XmlElement bodyElem, XmlNamespaceManager nsmgr)
        {
            Body = new TtmlBody(bodyElem, nsmgr, this);

            TtmlSize? extent = Body.Style?.Extent ?? Body.Region?.Style?.Extent;
            if (extent != null && extent.Value.Width.Unit == TtmlUnit.Pixels && extent.Value.Height.Unit == TtmlUnit.Pixels)
                VideoDimensions = new Size((int)extent.Value.Width.Value, (int)extent.Value.Height.Value);
        }

        private List<Line> ConvertTtmlToCommon()
        {
            TtmlResolutionContext initialContext = TtmlResolutionContext.CreateInitialContext(this);
            TtmlResolutionContext bodyContext = TtmlResolutionContext.Extend(initialContext, null, Body);
            return ConvertTtmlToCommon(Body, bodyContext) as List<Line> ?? new List<Line>();
        }

        private object ConvertTtmlToCommon(TtmlContent content, TtmlResolutionContext context)
        {
            return content switch
                   {
                       TtmlParagraph paragraph => ConvertTtmlParagraphToLine(paragraph, context),
                       TtmlSpan span => ConvertTtmlSpanToSections(span, context),
                       _ => ConvertTtmlChildren<Line>(content, context)
                   };
        }

        private Line ConvertTtmlParagraphToLine(TtmlParagraph paragraph, TtmlResolutionContext context)
        {
            AnchorPoint anchorPoint = GetAnchorPoint(context.Style.DisplayAlign, context.Style.TextAlign);

            PointF? position = null;
            if (context.Style.Position != null)
            {
                anchorPoint = GetAnchorPoint(context.Style.Position.Value.HBase, context.Style.Position.Value.VBase);
                position = context.Style.Position.Value.Resolve(context);
            }
            else if (context.Style.Origin != null)
            {
                PointF origin = (PointF)context.Style.Origin.Value.Resolve(context);
                SizeF extent = context.Style.Extent?.Resolve(context) ?? VideoDimensions;

                float x = context.Style.TextAlign switch
                          {
                              TtmlTextAlign.Left => origin.X,
                              TtmlTextAlign.Start => origin.X,
                              TtmlTextAlign.Justify => origin.X,
                              TtmlTextAlign.Center => origin.X + extent.Width / 2,
                              TtmlTextAlign.Right => origin.X + extent.Width,
                              TtmlTextAlign.End => origin.X + extent.Width
                          };
                float y = context.Style.DisplayAlign switch
                          {
                              TtmlDisplayAlign.Before => origin.Y,
                              TtmlDisplayAlign.Justify => origin.Y,
                              TtmlDisplayAlign.Center => origin.Y + extent.Height / 2,
                              TtmlDisplayAlign.After => origin.Y + extent.Height
                          };
                position = new PointF(x, y);
            }

            (HorizontalTextDirection hDir, VerticalTextType vDir) = GetTextDirections(context.Style.WritingMode, context.Style.TextOrientation, context.Style.Direction);

            Line line = new Line(context.BeginTime, context.EndTime)
                        {
                            AnchorPoint = anchorPoint,
                            Position = position,
                            HorizontalTextDirection = hDir,
                            VerticalTextType = vDir
                        };

            line.Sections.AddRange(ConvertTtmlChildren<Section>(paragraph, context));
            MergeIdenticallyFormattedSections(line);
            CutWhitespace(line);
            return line;
        }

        private List<Section> ConvertTtmlSpanToSections(TtmlSpan span, TtmlResolutionContext context)
        {
            if (context.Style.RubyMode == TtmlRubyMode.Container)
                return ConvertRubyTtmlSpanToSections(span, context);

            if (span.Children.Count > 0)
                return ConvertTtmlChildren<Section>(span, context);

            TtmlStyle style = context.Style;
            if (string.IsNullOrEmpty(span.Text) || style.Display == TtmlDisplayMode.None)
                return new List<Section>();

            float defaultFontSize = (Body.Style ?? Body.Region?.Style ?? InitialStyle).FontSize.Resolve(context, TtmlProgression.Inline);
            Section section = new Section(span.Text)
                              {
                                  Font = ConvertTtmlFontFamily(style.FontFamilies),
                                  Scale = style.FontSize.Resolve(context, TtmlProgression.Inline) / defaultFontSize,
                                  Bold = (style.FontWeight == TtmlFontWeight.Bold),
                                  Italic = (style.FontStyle == TtmlFontStyle.Italic || style.FontStyle == TtmlFontStyle.Oblique),
                                  Underline = (style.TextDecoration & TtmlTextDecoration.Underline) != 0,
                                  Offset = (style.FontVariant & (TtmlFontVariant.Sub | TtmlFontVariant.Super)) switch
                                           {
                                               TtmlFontVariant.Super => OffsetType.Superscript,
                                               TtmlFontVariant.Sub => OffsetType.Subscript,
                                               _ => OffsetType.Regular
                                           },
                                  ForeColor = ColorUtil.ChangeAlpha(style.Color, (int)(style.Color.A * style.Opacity)),
                                  BackColor = ColorUtil.ChangeAlpha(style.BackgroundColor, (int)(style.BackgroundColor.A * style.Opacity)),
                                  Packed = style.TextCombine == TtmlTextCombine.All,
                                  RubyPart = style.RubyMode switch
                                             {
                                                 TtmlRubyMode.Base => RubyPart.Base,
                                                 TtmlRubyMode.Delimiter => RubyPart.Parenthesis,
                                                 TtmlRubyMode.Text => style.RubyPosition == TtmlRubyPosition.After ? RubyPart.TextAfter : RubyPart.TextBefore,
                                                 _ => RubyPart.None
                                             }
                              };

            if (style.Visibility == TtmlVisibility.Hidden)
                section.ForeColor = Color.Transparent;

            ConvertTtmlShadows(style, section);

            return new List<Section> { section };
        }

        private List<Section> ConvertRubyTtmlSpanToSections(TtmlSpan span, TtmlResolutionContext context)
        {
            List<Section> baseSections = new List<Section>();
            List<Section> openParenthesisSections = new List<Section>();
            List<Section> textSections = new List<Section>();
            List<Section> closeParenthesisSections = new List<Section>();
            ConvertRubyTtmlSpanToSections(span, context, baseSections, openParenthesisSections, textSections, closeParenthesisSections);

            int numParts = Math.Min(baseSections.Count, textSections.Count);

            while (baseSections.Count > numParts)
            {
                baseSections[numParts - 1].Text += baseSections[numParts].Text;
                baseSections.RemoveAt(numParts);
            }

            while (textSections.Count > numParts)
            {
                textSections[numParts - 1].Text += textSections[numParts].Text;
                textSections.RemoveAt(numParts);
            }

            if (closeParenthesisSections.Count < textSections.Count)
            {
                Section closeParenthesisSection = (Section)textSections.Last().Clone();
                closeParenthesisSection.Text = ")";
                closeParenthesisSection.RubyPart = RubyPart.Parenthesis;
                closeParenthesisSections.Add(closeParenthesisSection);
            }

            List<Section> result = new List<Section>();
            for (int i = 0; i < numParts; i++)
            {
                result.Add(baseSections[i]);
                result.Add(openParenthesisSections[i]);
                result.Add(textSections[i]);
                result.Add(closeParenthesisSections[i]);
            }

            return result;
        }

        private void ConvertRubyTtmlSpanToSections(
            TtmlSpan span,
            TtmlResolutionContext context,
            List<Section> baseSections,
            List<Section> openParenthesisSections,
            List<Section> textSections,
            List<Section> closeParenthesisSections)
        {
            TtmlResolutionContext prevChildContext = null;
            bool textContainerHandled = false;

            foreach (TtmlSpan childSpan in span.Children.OfType<TtmlSpan>())
            {
                TtmlResolutionContext childContext = TtmlResolutionContext.Extend(context, prevChildContext, childSpan);
                prevChildContext = childContext;

                if (childContext.Style.RubyMode == TtmlRubyMode.Base)
                {
                    List<Section> childBaseSections = ConvertTtmlSpanToSections(childSpan, childContext);
                    CollapseSections(childBaseSections);
                    baseSections.AddRange(childBaseSections);
                }
                else if (childContext.Style.RubyMode == TtmlRubyMode.Delimiter)
                {
                    List<Section> childDelimiterSections = ConvertTtmlSpanToSections(childSpan, childContext);
                    CollapseSections(childDelimiterSections);
                    if (closeParenthesisSections.Count < textSections.Count)
                        closeParenthesisSections.AddRange(childDelimiterSections);
                    else
                        openParenthesisSections.AddRange(childDelimiterSections);
                }
                else if (childContext.Style.RubyMode == TtmlRubyMode.Text)
                {
                    if (closeParenthesisSections.Count < textSections.Count)
                    {
                        Section closeParenthesisSection = (Section)textSections.Last().Clone();
                        closeParenthesisSection.Text = ")";
                        closeParenthesisSection.RubyPart = RubyPart.Parenthesis;
                        closeParenthesisSections.Add(closeParenthesisSection);
                    }

                    List<Section> childTextSections = ConvertTtmlSpanToSections(childSpan, childContext);
                    CollapseSections(childTextSections);
                    textSections.AddRange(childTextSections);

                    if (openParenthesisSections.Count < textSections.Count)
                    {
                        Section openParenthesisSection = (Section)textSections.Last().Clone();
                        openParenthesisSection.Text = "(";
                        openParenthesisSection.RubyPart = RubyPart.Parenthesis;
                        openParenthesisSections.Add(openParenthesisSection);
                    }
                }
                else
                {
                    if (childContext.Style.RubyMode == TtmlRubyMode.TextContainer)
                    {
                        if (textContainerHandled)
                            continue;

                        textContainerHandled = true;
                    }

                    ConvertRubyTtmlSpanToSections(childSpan, childContext, baseSections, openParenthesisSections, textSections, closeParenthesisSections);
                }
            }
        }

        private static void CollapseSections(List<Section> sections)
        {
            while (sections.Count > 1)
            {
                sections[0].Text += sections[1].Text;
                sections.RemoveAt(1);
            }
        }

        private static string ConvertTtmlFontFamily(List<string> families)
        {
            string family = families.FirstOrDefault();
            return family switch
                   {
                       "monospace" => "Lucida Console",
                       "monospaceSansSerif" => "Lucida Console",
                       "monospaceSerif" => "Courier New",
                       "serif" => "Times New Roman",
                       "proportionalSerif" => "Times New Roman",
                       _ => family
                   };
        }

        private static void ConvertTtmlShadows(TtmlStyle style, Section section)
        {
            if (style.TextOutline != null && style.TextOutline.Thickness.Value > 0)
            {
                Color color = style.TextOutline.Color;
                if (color.IsEmpty)
                    color = style.Color;

                if (color.A > 0)
                    section.ShadowColors[ShadowType.Glow] = color;
            }

            foreach (TtmlShadow shadow in style.TextShadows)
            {
                Color color = shadow.Color;
                if (color.IsEmpty)
                    color = style.Color;

                if (color.A == 0)
                    continue;

                if (shadow.Offset.Width.Value == 0 && shadow.Offset.Height.Value == 0)
                {
                    if (shadow.BlurRadius.Value > 0)
                        section.ShadowColors[ShadowType.Glow] = color;
                }
                else if (shadow.BlurRadius.Value == 0)
                {
                    section.ShadowColors[ShadowType.HardShadow] = color;
                }
                else
                {
                    section.ShadowColors[ShadowType.SoftShadow] = color;
                }
            }
        }

        private List<T> ConvertTtmlChildren<T>(TtmlContent content, TtmlResolutionContext context)
        {
            List<T> result = new List<T>();
            TtmlResolutionContext prevChildContext = null;
            foreach (TtmlContent ttmlChild in content.Children)
            {
                TtmlResolutionContext childContext = TtmlResolutionContext.Extend(context, prevChildContext, ttmlChild);
                object translation = ConvertTtmlToCommon(ttmlChild, childContext);
                switch (translation)
                {
                    case List<T> commonChildList:
                        result.AddRange(commonChildList);
                        break;

                    case T commonChild:
                        result.Add(commonChild);
                        break;
                }
                prevChildContext = childContext;
            }
            return result;
        }

        private static void CutWhitespace(Line line)
        {
            while (line.Sections.Count > 0 && string.IsNullOrWhiteSpace(line.Sections[0].Text))
            {
                line.Sections.RemoveAt(0);
            }

            if (line.Sections.Count > 0)
                line.Sections[0].Text = line.Sections[0].Text.TrimStart(' ');

            while (line.Sections.Count > 0 && string.IsNullOrWhiteSpace(line.Sections[line.Sections.Count - 1].Text))
            {
                line.Sections.RemoveAt(line.Sections.Count - 1);
            }

            if (line.Sections.Count > 0)
                line.Sections[line.Sections.Count - 1].Text = line.Sections[line.Sections.Count - 1].Text.TrimEnd(' ');

            foreach (Section section in line.Sections)
            {
                section.Text = Regex.Replace(section.Text, @"  +", " ");
            }
        }

        public override void Save(TextWriter textWriter)
        {
            MergeIdenticallyFormattedSections();

            Dictionary<Line, int> regions = ExtractAttributes(Lines, new RegionAttributeComparer());
            Dictionary<Section, int> styles = ExtractAttributes(Lines.SelectMany(l => l.Sections), new SectionFormatComparer());

            using XmlWriter writer = XmlWriter.Create(textWriter, new XmlWriterSettings { CloseOutput = false });
            writer.WriteStartElement("tt", Namespaces.Tt);
            writer.WriteAttributeString("xmlns", "tts", null, Namespaces.Tts);
            writer.WriteAttributeString("tts", "extent", null, new TtmlSize(VideoDimensions.Width, TtmlUnit.Pixels, VideoDimensions.Height, TtmlUnit.Pixels).ToString());

            WriteHead(writer, regions, styles);
            WriteBody(writer, regions, styles);

            writer.WriteEndElement();
        }

        private void WriteHead(XmlWriter writer, Dictionary<Line, int> regions, Dictionary<Section, int> styles)
        {
            writer.WriteStartElement("head");
            WriteRegions(writer, regions);
            WriteStyles(writer, styles);
            writer.WriteEndElement();
        }

        private void WriteRegions(XmlWriter writer, Dictionary<Line, int> regions)
        {
            writer.WriteStartElement("layout");
            foreach ((Line region, int regionId) in regions)
            {
                WriteRegion(writer, regionId, region);
            }
            writer.WriteEndElement();
        }

        private void WriteRegion(XmlWriter writer, int regionId, Line region)
        {
            writer.WriteStartElement("region");
            writer.WriteAttributeString("xml", "id", null, "region" + regionId);

            (TtmlDisplayAlign displayAlign, TtmlTextAlign textAlign) = GetAlignments(region.AnchorPoint);
            if (displayAlign != TtmlDisplayAlign.Before)
                writer.WriteAttributeString("tts", "displayAlign", null, displayAlign.ToString().ToLower());

            if (textAlign != TtmlTextAlign.Left)
                writer.WriteAttributeString("tts", "textAlign", null, textAlign.ToString().ToLower());

            PointF position = region.Position ?? GetDefaultPosition(region.AnchorPoint);
            float originX = textAlign switch
                            {
                                TtmlTextAlign.Left => position.X,
                                TtmlTextAlign.Center => position.X - VideoDimensions.Width / 2,
                                TtmlTextAlign.Right => position.X - VideoDimensions.Width
                            };
            float originY = displayAlign switch
                            {
                                TtmlDisplayAlign.Before => position.Y,
                                TtmlDisplayAlign.Center => position.Y - VideoDimensions.Height / 2,
                                TtmlDisplayAlign.After => position.Y - VideoDimensions.Height
                            };
            writer.WriteAttributeString("tts", "origin", null, new TtmlSize(originX, TtmlUnit.Pixels, originY, TtmlUnit.Pixels).ToString());

                (TtmlWritingMode writingMode, TtmlTextOrientation orientation, TtmlDirection direction) = GetWritingModes(region.HorizontalTextDirection, region.VerticalTextType);
            if (writingMode != TtmlWritingMode.Lr)
                writer.WriteAttributeString("tts", "writingMode", null, writingMode.ToString().ToLower());

            if (orientation != TtmlTextOrientation.Mixed)
                writer.WriteAttributeString("tts", "textOrientation", null, orientation.ToString().ToLower());

            writer.WriteAttributeString("tts", "direction", null, direction.ToString().ToLower());

            writer.WriteEndElement();
        }

        private void WriteStyles(XmlWriter writer, Dictionary<Section, int> styles)
        {
            writer.WriteStartElement("styling");
            foreach ((Section style, int styleId) in styles)
            {
                WriteStyle(writer, styleId, style);
            }
            writer.WriteEndElement();
        }

        private void WriteStyle(XmlWriter writer, int styleId, Section style)
        {
            writer.WriteStartElement("style");
            writer.WriteAttributeString("xml", "id", null, "style" + styleId);

            if (style.Bold)
                writer.WriteAttributeString("tts", "fontWeight", null, "bold");

            if (style.Italic)
                writer.WriteAttributeString("tts", "fontStyle", null, "italic");

            if (style.Underline)
                writer.WriteAttributeString("tts", "textDecoration", null, "underline");

            TtmlFontVariant fontVariant = GetFontVariant(style.Offset);
            if (fontVariant != TtmlFontVariant.Normal)
                writer.WriteAttributeString("tts", "fontVariant", null, fontVariant.ToString().ToLower());

            if (style.Font != null)
                writer.WriteAttributeString("tts", "fontFamily", null, style.Font);

            if (!style.BackColor.IsEmpty)
                writer.WriteAttributeString("tts", "backgroundColor", null, TtmlColor.ToString(style.BackColor));

            if (!style.ForeColor.IsEmpty)
                writer.WriteAttributeString("tts", "color", null, TtmlColor.ToString(style.ForeColor));

            if (style.Scale != 1.0f)
                writer.WriteAttributeString("tts", "fontSize", null, new TtmlLength(style.Scale, TtmlUnit.Em).ToString());

            if (style.ShadowColors.ContainsKey(ShadowType.Glow))
                writer.WriteAttributeString("tts", "textOutline", null, new TtmlOutline(style.ShadowColors[ShadowType.Glow], new TtmlLength(5, TtmlUnit.Percent), new TtmlLength()).ToString());

            string shadows = GetTextShadows(style.ShadowColors);
            if (!string.IsNullOrEmpty(shadows))
                writer.WriteAttributeString("tts", "textShadow", null, shadows);

            if (style.Packed)
                writer.WriteAttributeString("tts", "textCombine", null, "all");

            switch (style.RubyPart)
            {
                case RubyPart.Base:
                    writer.WriteAttributeString("tts", "ruby", null, "base");
                    break;

                case RubyPart.Parenthesis:
                    writer.WriteAttributeString("tts", "ruby", null, "delimiter");
                    break;

                case RubyPart.TextBefore:
                    writer.WriteAttributeString("tts", "ruby", null, "text");
                    break;

                case RubyPart.TextAfter:
                    writer.WriteAttributeString("tts", "ruby", null, "text");
                    writer.WriteAttributeString("tts", "rubyPosition", null, "after");
                    break;
            }

            writer.WriteEndElement();
        }

        private void WriteBody(XmlWriter writer, Dictionary<Line, int> regionIds, Dictionary<Section, int> styleIds)
        {
            writer.WriteStartElement("body");
            writer.WriteStartElement("div");

            foreach (Line line in Lines)
            {
                WriteLine(writer, line, regionIds, styleIds);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteLine(XmlWriter writer, Line line, Dictionary<Line, int> regionIds, Dictionary<Section, int> styleIds)
        {
            writer.WriteStartElement("p");
            writer.WriteAttributeString("begin", TtmlTime.ToString(line.Start));
            writer.WriteAttributeString("end", TtmlTime.ToString(line.End));
            writer.WriteAttributeString("region", "region" + regionIds[line]);

            int i = 0;
            while (i < line.Sections.Count)
            {
                Section section = line.Sections[i];
                if (section.RubyPart == RubyPart.Base)
                {
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("tts", "ruby", null, "container");
                    WriteSection(writer, line.Sections[i + 0], styleIds);
                    WriteSection(writer, line.Sections[i + 1], styleIds);
                    WriteSection(writer, line.Sections[i + 2], styleIds);
                    WriteSection(writer, line.Sections[i + 3], styleIds);
                    writer.WriteEndElement();
                    i += 4;
                }
                else
                {
                    WriteSection(writer, section, styleIds);
                    i++;
                }
            }

            writer.WriteEndElement();
        }

        private void WriteSection(XmlWriter writer, Section section, Dictionary<Section, int> styleIds)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("style", "style" + styleIds[section]);

            string[] lines = section.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                writer.WriteValue(lines[i]);
                if (i < lines.Length - 1)
                {
                    writer.WriteStartElement("br");
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        private static AnchorPoint GetAnchorPoint(TtmlDisplayAlign displayAlign, TtmlTextAlign textAlign)
        {
            displayAlign = displayAlign switch
                           {
                               TtmlDisplayAlign.Justify => TtmlDisplayAlign.Before,
                               _ => displayAlign
                           };
            textAlign = textAlign switch
                        {
                            TtmlTextAlign.Start => TtmlTextAlign.Left,
                            TtmlTextAlign.Justify => TtmlTextAlign.Left,
                            TtmlTextAlign.End => TtmlTextAlign.Right,
                            _ => textAlign
                        };

            return displayAlign switch
                   {
                       TtmlDisplayAlign.Before => textAlign switch
                                                  {
                                                      TtmlTextAlign.Left => AnchorPoint.TopLeft,
                                                      TtmlTextAlign.Center => AnchorPoint.TopCenter,
                                                      TtmlTextAlign.Right => AnchorPoint.TopRight
                                                  },
                       TtmlDisplayAlign.Center => textAlign switch
                                                  {
                                                      TtmlTextAlign.Left => AnchorPoint.MiddleLeft,
                                                      TtmlTextAlign.Center => AnchorPoint.Center,
                                                      TtmlTextAlign.Right => AnchorPoint.MiddleRight
                                                  },
                       TtmlDisplayAlign.After => textAlign switch
                                                 {
                                                     TtmlTextAlign.Left => AnchorPoint.BottomLeft,
                                                     TtmlTextAlign.Center => AnchorPoint.BottomCenter,
                                                     TtmlTextAlign.Right => AnchorPoint.BottomRight
                                                 }
                   };
        }

        private static (TtmlDisplayAlign, TtmlTextAlign) GetAlignments(AnchorPoint anchorPoint)
        {
            return anchorPoint switch
                   {
                       AnchorPoint.TopLeft => (TtmlDisplayAlign.Before, TtmlTextAlign.Left),
                       AnchorPoint.TopCenter => (TtmlDisplayAlign.Before, TtmlTextAlign.Center),
                       AnchorPoint.TopRight => (TtmlDisplayAlign.Before, TtmlTextAlign.Right),
                       AnchorPoint.MiddleLeft => (TtmlDisplayAlign.Center, TtmlTextAlign.Left),
                       AnchorPoint.Center => (TtmlDisplayAlign.Center, TtmlTextAlign.Center),
                       AnchorPoint.MiddleRight => (TtmlDisplayAlign.Center, TtmlTextAlign.Right),
                       AnchorPoint.BottomLeft => (TtmlDisplayAlign.After, TtmlTextAlign.Left),
                       AnchorPoint.BottomCenter => (TtmlDisplayAlign.After, TtmlTextAlign.Center),
                       AnchorPoint.BottomRight => (TtmlDisplayAlign.After, TtmlTextAlign.Right)
                   };
        }

        private static AnchorPoint GetAnchorPoint(TtmlPosHBase hBase, TtmlPosVBase vBase)
        {
            return vBase switch
                   {
                       TtmlPosVBase.Top => hBase switch
                                           {
                                               TtmlPosHBase.Left => AnchorPoint.TopLeft,
                                               TtmlPosHBase.Center => AnchorPoint.TopCenter,
                                               TtmlPosHBase.Right => AnchorPoint.TopRight
                                           },
                       TtmlPosVBase.Center => hBase switch
                                              {
                                                  TtmlPosHBase.Left => AnchorPoint.MiddleLeft,
                                                  TtmlPosHBase.Center => AnchorPoint.Center,
                                                  TtmlPosHBase.Right => AnchorPoint.MiddleRight
                                              },
                       TtmlPosVBase.Bottom => hBase switch
                                              {
                                                  TtmlPosHBase.Left => AnchorPoint.BottomLeft,
                                                  TtmlPosHBase.Center => AnchorPoint.BottomCenter,
                                                  TtmlPosHBase.Right => AnchorPoint.BottomRight
                                              }
                   };
        }

        private static (HorizontalTextDirection, VerticalTextType) GetTextDirections(TtmlWritingMode writingMode, TtmlTextOrientation textOrientation, TtmlDirection direction)
        {
            HorizontalTextDirection horizontal;
            VerticalTextType vertical;

            if (writingMode == TtmlWritingMode.Tbrl || writingMode == TtmlWritingMode.Tb)
            {
                horizontal = HorizontalTextDirection.RightToLeft;
                vertical = textOrientation == TtmlTextOrientation.Upright ? VerticalTextType.Positioned : VerticalTextType.Rotated;
            }
            else if (writingMode == TtmlWritingMode.Tblr)
            {
                horizontal = HorizontalTextDirection.LeftToRight;
                vertical = textOrientation == TtmlTextOrientation.Upright ? VerticalTextType.Positioned : VerticalTextType.Rotated;
            }
            else if (writingMode == TtmlWritingMode.Rltb ||
                     writingMode == TtmlWritingMode.Rl ||
                     direction == TtmlDirection.Rtl)
            {
                horizontal = HorizontalTextDirection.RightToLeft;
                vertical = VerticalTextType.None;
            }
            else
            {
                horizontal = HorizontalTextDirection.LeftToRight;
                vertical = VerticalTextType.None;
            }

            return (horizontal, vertical);
        }

        private static (TtmlWritingMode, TtmlTextOrientation, TtmlDirection) GetWritingModes(HorizontalTextDirection horizontal, VerticalTextType vertical)
        {
            switch (vertical)
            {
                case VerticalTextType.None:
                    return (
                               horizontal == HorizontalTextDirection.RightToLeft ? TtmlWritingMode.Rl : TtmlWritingMode.Lr,
                               TtmlTextOrientation.Mixed,
                               horizontal == HorizontalTextDirection.RightToLeft ? TtmlDirection.Rtl : TtmlDirection.Ltr
                           );

                case VerticalTextType.Rotated:
                    return (
                               horizontal == HorizontalTextDirection.RightToLeft ? TtmlWritingMode.Tbrl : TtmlWritingMode.Tblr,
                               TtmlTextOrientation.Sideways,
                               TtmlDirection.Ltr
                           );

                case VerticalTextType.Positioned:
                    return (
                               horizontal == HorizontalTextDirection.RightToLeft ? TtmlWritingMode.Tbrl : TtmlWritingMode.Tblr,
                               TtmlTextOrientation.Upright,
                               TtmlDirection.Ltr
                           );

                default:
                    throw new ArgumentException();
            }
        }

        private static TtmlFontVariant GetFontVariant(OffsetType offsetType)
        {
            return offsetType switch
                   {
                       OffsetType.Subscript => TtmlFontVariant.Sub,
                       OffsetType.Superscript => TtmlFontVariant.Super,
                       _ => TtmlFontVariant.Normal
                   };
        }

        private static string GetTextShadows(Dictionary<ShadowType, Color> shadows)
        {
            List<TtmlShadow> ttmlShadows = new List<TtmlShadow>();
            foreach ((ShadowType type, Color color) in shadows)
            {
                switch (type)
                {
                    case ShadowType.Bevel:
                        ttmlShadows.Add(new TtmlShadow(new TtmlSize(-4, TtmlUnit.Percent, -4, TtmlUnit.Percent), new TtmlLength(), color));
                        ttmlShadows.Add(new TtmlShadow(new TtmlSize(4, TtmlUnit.Percent, 4, TtmlUnit.Percent), new TtmlLength(), color));
                        break;

                    case ShadowType.HardShadow:
                        ttmlShadows.Add(new TtmlShadow(new TtmlSize(4, TtmlUnit.Percent, 4, TtmlUnit.Percent), new TtmlLength(), color));
                        break;

                    case ShadowType.SoftShadow:
                        ttmlShadows.Add(new TtmlShadow(new TtmlSize(4, TtmlUnit.Percent, 4, TtmlUnit.Percent), new TtmlLength(2, TtmlUnit.Percent), color));
                        break;
                }
            }
            return string.Join(", ", ttmlShadows);
        }

        private static (int, int) ParseIntPair(string str)
        {
            Match match = Regex.Match(str, @"^(\d+)\s+(\d+)$");
            if (!match.Success)
                throw new FormatException();

            return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }

        private class RegionAttributeComparer : IEqualityComparer<Line>
        {
            public bool Equals(Line x, Line y)
            {
                return x.AnchorPoint == y.AnchorPoint &&
                       x.Position == y.Position &&
                       x.HorizontalTextDirection == y.HorizontalTextDirection &&
                       x.VerticalTextType == y.VerticalTextType;
            }

            public int GetHashCode(Line obj)
            {
                return obj.AnchorPoint.GetHashCode() ^
                       obj.Position.GetHashCode() ^
                       obj.HorizontalTextDirection.GetHashCode() ^
                       obj.VerticalTextType.GetHashCode();
            }
        }
    }
}
