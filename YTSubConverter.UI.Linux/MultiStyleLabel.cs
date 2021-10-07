using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using YTSubConverter.Shared.Util;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    internal class MultiStyleLabel : Overlay
    {
        private string _markup;

        public MultiStyleLabel(Box background)
        {
            Child = background;
        }

        public string Markup
        {
            get { return _markup; }
            set
            {
                _markup = value;
                if (Parent != null)
                    UpdateAsync();
            }
        }

        protected override void OnParentSet(Widget previous_parent)
        {
            base.OnParentSet(previous_parent);
            if (Parent != null)
                UpdateAsync();
        }

        private async Task UpdateAsync()
        {
            foreach (Label label in Children.OfType<Label>())
            {
                Remove(label);
            }

            ParseMarkup(_markup, out string text, out List<MarkupSegment> markupSegments);
            List<LayoutSegment> layoutSegments = await GetLayoutSegmentsAsync(text, markupSegments);
            CreateSegmentLabels(text, markupSegments, layoutSegments);
        }

        private static void ParseMarkup(string markup, out string text, out List<MarkupSegment> segments)
        {
            XDocument doc = XDocument.Parse(markup.StartsWith("<markup>") ? markup : $"<markup>{markup}</markup>");

            text = doc.Root.Value;

            int offset = 0;
            segments = new List<MarkupSegment>();
            foreach (XNode node in doc.Root.Nodes())
            {
                MarkupSegment segment;

                switch (node)
                {
                    case XText textNode:
                        segment = new MarkupSegment(offset, textNode.Value.Length, null);
                        break;

                    case XElement elem:
                        if (elem.Name != "span")
                            throw new NotSupportedException("Unsupported element in markup");

                        string css = elem.Attribute("css")?.Value;
                        segment = new MarkupSegment(offset, elem.Value.Length, css);
                        break;

                    default:
                        throw new NotSupportedException("Unsupported node in markup");
                }

                segments.Add(segment);
                offset += segment.Length;
            }
        }

        private Task<List<LayoutSegment>> GetLayoutSegmentsAsync(string text, List<MarkupSegment> markupSegments)
        {
            string markup = CreateLayoutMarkup(text, markupSegments);
            Label layoutLabel = new Label
                                {
                                    UseMarkup = true,
                                    Markup = markup,
                                    Halign = Align.Center,
                                    Valign = Align.Center,
                                    Justify = Justification.Center,
                                    Wrap = true,
                                    Visible = true
                                };

            var completionSource = new TaskCompletionSource<List<LayoutSegment>>();
            layoutLabel.Drawn += HandleDrawn;
            AddOverlay(layoutLabel);
            return completionSource.Task;

            void HandleDrawn(object o, DrawnArgs args)
            {
                layoutLabel.Drawn -= HandleDrawn;

                layoutLabel.TranslateCoordinates(Child, 0, 0, out int baseX, out int baseY);

                List<LayoutSegment> layoutSegments = new List<LayoutSegment>();
                Pango.LayoutIter iter = layoutLabel.Layout.Iter;
                do
                {
                    if (iter.Run.Item == null)
                        continue;

                    int offset = iter.Run.Item.Offset;
                    int length = iter.Run.Item.Length;

                    iter.GetRunExtents(out Pango.Rectangle inkRect, out Pango.Rectangle logicalRect);
                    logicalRect = logicalRect.ToPixelsInclusive();
                    Point position = new Point(baseX + logicalRect.X, baseY + logicalRect.Y);

                    layoutSegments.Add(new LayoutSegment(offset, length, position));
                } while (iter.NextRun());

                Remove(layoutLabel);

                completionSource.SetResult(layoutSegments);
            }
        }

        private static string CreateLayoutMarkup(string text, List<MarkupSegment> markupSegments)
        {
            StringBuilder markup = new StringBuilder();
            foreach (MarkupSegment markupSegment in markupSegments)
            {
                XElement spanElem = new XElement("span") { Value = text.Substring(markupSegment.Offset, markupSegment.Length) };
                spanElem.SetAttributeValue("color", $"#{markupSegment.Offset:X06}");
                spanElem.SetAttributeValue("font", CreatePangoFontDescFromCss(markupSegment.CssProperties));
                markup.Append(spanElem.ToString());
            }
            return markup.ToString();
        }

        private static string CreatePangoFontDescFromCss(string css)
        {
            Dictionary<string, string> cssDict = ParseCssProperties(css);
            List<string> fontItems = new List<string>();
            foreach (string keyword in new[] { "font-family", "font-weight", "font-style", "font-size" })
            {
                string value = cssDict.GetOrDefault(keyword);
                if (value != null)
                    fontItems.Add(value.Replace("\"", ""));
            }

            return string.Join(" ", fontItems);
        }

        private void CreateSegmentLabels(string text, List<MarkupSegment> markupSegments, List<LayoutSegment> layoutSegments)
        {
            List<List<LayoutSegment>> layoutLines =
                layoutSegments.GroupBy(s => s.Position.Y)
                              .Select(g => g.ToList())
                              .ToList();

            for (int lineIdx = 0; lineIdx < layoutLines.Count; lineIdx++)
            {
                List<LayoutSegment> layoutLine = layoutLines[lineIdx];

                for (int segmentIdx = 0; segmentIdx < layoutLine.Count; segmentIdx++)
                {
                    LayoutSegment layoutSegment = layoutLine[segmentIdx];

                    Label label = new Label
                                  {
                                      Text = text.Substring(layoutSegment.Offset, layoutSegment.Length),
                                      Halign = Align.Start,
                                      Valign = Align.Start,
                                      Visible = true
                                  };

                    MarkupSegment markupSegment = markupSegments.Last(s => s.Offset <= layoutSegment.Offset);

                    Dictionary<string, string> cssDict = ParseCssProperties(markupSegment.CssProperties);
                    cssDict["margin"] = $"{layoutSegment.Position.Y}px 0px 0px {layoutSegment.Position.X}px";
                    AdjustPadding(cssDict, lineIdx, layoutLines.Count, segmentIdx, layoutLine.Count);
                    label.ApplyCss("* {" + FormatCssProperties(cssDict) + "}");

                    AddOverlay(label);
                }
            }
        }

        private static void AdjustPadding(Dictionary<string, string> cssDict, int lineIdx, int numLines, int segmentIdx, int numSegments)
        {
            int[] margin = ParseSpacing(cssDict.GetOrDefault("margin"));
            int[] padding = ParseSpacing(cssDict.GetOrDefault("padding"));

            // First line
            if (lineIdx == 0)
                margin[0] -= padding[0];
            else
                padding[0] = 0;

            // Last line
            if (lineIdx == numLines - 1)
                ;
            else
                padding[2] = 0;

            // Start of line
            if (segmentIdx == 0)
                margin[3] -= padding[3];
            else
                padding[3] = 0;

            // End of line
            if (segmentIdx == numSegments - 1)
                ;
            else
                padding[1] = 0;

            cssDict["margin"] = FormatSpacing(margin);
            cssDict["padding"] = FormatSpacing(padding);
        }

        private static Dictionary<string, string> ParseCssProperties(string properties)
        {
            return Regex.Matches(properties, @"([-\w]+)\s*:\s*([^;]+)\s*;")
                        .Cast<Match>()
                        .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        }

        private static string FormatCssProperties(Dictionary<string, string> properties)
        {
            return string.Join("; ", properties.Select(p => $"{p.Key}: {p.Value}"));
        }

        private static int[] ParseSpacing(string spacing)
        {
            if (spacing == null)
                return new[] { 0, 0, 0, 0 };

            Match match = Regex.Match(spacing, @"^(?:\s*(-?\d+)px)+\s*$");
            if (!match.Success)
                throw new ArgumentException();

            int[] items = match.Groups[1].Captures
                                         .Cast<Capture>()
                                         .Select(c => int.Parse(c.Value))
                                         .ToArray();

            if (items.Length == 1)
                items = new[] { items[0], items[0], items[0], items[0] };
            else if (items.Length == 2)
                items = new[] { items[0], items[1], items[0], items[1] };
            else if (items.Length == 3)
                items = new[] { items[0], items[1], items[2], items[1] };

            return items;
        }

        private static string FormatSpacing(int[] items)
        {
            return string.Join(" ", items.Select(i => $"{i}px"));
        }

        private struct MarkupSegment
        {
            public MarkupSegment(int offset, int length, string cssProperties)
            {
                Offset = offset;
                Length = length;
                CssProperties = cssProperties;
            }

            public int Offset;

            public int Length;

            public string CssProperties;
        }

        private struct LayoutSegment
        {
            public LayoutSegment(int offset, int length, Point position)
            {
                Offset = offset;
                Length = length;
                Position = position;
            }

            public int Offset;

            public int Length;

            public Point Position;
        }
    }
}
