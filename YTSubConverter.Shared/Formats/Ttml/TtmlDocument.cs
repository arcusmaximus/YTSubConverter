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
            nsmgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
            nsmgr.AddNamespace("tt", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/ttml#styling");

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
                position = (PointF)context.Style.Origin.Value.Resolve(context);
            }

            (HorizontalTextDirection hDir, VerticalTextType vDir) = GetTextDirections(context.Style);

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
                                  Italic = (style.FontStyle == TtmlFontStyle.Italic),
                                  Underline = (style.TextDecoration & TtmlTextDecoration.Underline) != 0,
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

        private static AnchorPoint GetAnchorPoint(TtmlDisplayAlign displayAlign, TtmlTextAlign textAlign)
        {
            displayAlign = displayAlign switch
                           {
                               TtmlDisplayAlign.Justify => TtmlDisplayAlign.Center,
                               _ => displayAlign
                           };
            textAlign = textAlign switch
                        {
                            TtmlTextAlign.Start => TtmlTextAlign.Left,
                            TtmlTextAlign.Justify => TtmlTextAlign.Center,
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

        private static (HorizontalTextDirection, VerticalTextType) GetTextDirections(TtmlStyle style)
        {
            HorizontalTextDirection horizontal;
            VerticalTextType vertical;

            if (style.WritingMode == TtmlWritingMode.Tbrl || style.WritingMode == TtmlWritingMode.Tb)
            {
                horizontal = HorizontalTextDirection.RightToLeft;
                vertical = style.TextOrientation == TtmlTextOrientation.Upright ? VerticalTextType.Positioned : VerticalTextType.Rotated;
            }
            else if (style.WritingMode == TtmlWritingMode.Tblr)
            {
                horizontal = HorizontalTextDirection.LeftToRight;
                vertical = style.TextOrientation == TtmlTextOrientation.Upright ? VerticalTextType.Positioned : VerticalTextType.Rotated;
            }
            else if (style.WritingMode == TtmlWritingMode.Rltb ||
                     style.WritingMode == TtmlWritingMode.Rl ||
                     style.Direction == TtmlDirection.Rtl)
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

        private static (int, int) ParseIntPair(string str)
        {
            Match match = Regex.Match(str, @"^(\d+)\s+(\d+)$");
            if (!match.Success)
                throw new FormatException();

            return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }
    }
}
