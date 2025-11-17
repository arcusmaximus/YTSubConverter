using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass
{
    internal class RubyVisualizer
    {
        private record SplitLineVisualization(List<AssLine> Lines, float Width, float Top, float Bottom);

        private readonly AssLine _inputLine;
        private readonly float _defaultLineHeight;
        private readonly ITextMeasurer _textMeasurer;
        private readonly Dictionary<string, float> _sizeFactors = new();

        private List<AssLine> _vizLines;
        private AssLine _lastBaseVizLine;
        private float _vizRight;
        private float _vizTop;
        private float _vizBottom;

        public RubyVisualizer(AssLine inputLine, float defaultLineHeight, ITextMeasurer textMeasurer)
        {
            _inputLine = inputLine;
            _defaultLineHeight = defaultLineHeight;
            _textMeasurer = textMeasurer;
        }

        public IEnumerable<AssLine> MakeVisualizationLines()
        {
            List<SplitLineVisualization> visualizations = new();
            foreach (AssLine splitLine in GetSplitLines())
            {
                visualizations.Add(MakeSplitLineVisualization(splitLine));
            }

            FixPositions(visualizations);
            return visualizations.SelectMany(v => v.Lines);
        }

        private IEnumerable<AssLine> GetSplitLines()
        {
            AssLine splitLine = new AssLine(_inputLine, false);
            foreach (AssSection section in _inputLine.Sections)
            {
                int newlineIdx = section.Text.IndexOf("\r\n");
                AssSection newSection = (AssSection)section.Clone();
                if (newlineIdx < 0)
                {
                    splitLine.Sections.Add(newSection);
                }
                else
                {
                    AssSection nextLineSection = (AssSection)newSection.Clone();
                    newSection.Text = newSection.Text.Substring(0, newlineIdx);
                    nextLineSection.Text = nextLineSection.Text.Substring(newlineIdx + 2);

                    if (newSection.Text.Length > 0)
                        splitLine.Sections.Add(newSection);

                    if (splitLine.Sections.Count > 0)
                    {
                        yield return splitLine;
                        splitLine = new AssLine(_inputLine, false);
                    }

                    if (nextLineSection.Text.Length > 0)
                        splitLine.Sections.Add(nextLineSection);
                }
            }

            if (splitLine.Sections.Count > 0)
                yield return splitLine;
        }

        private SplitLineVisualization MakeSplitLineVisualization(AssLine splitLine)
        {
            _vizLines = new();
            _lastBaseVizLine = null;
            _vizRight = 0;
            _vizTop = 0;
            _vizBottom = 0;

            int sectionIdx = 0;
            while (sectionIdx < splitLine.Sections.Count)
            {
                AssSection baseSection = (AssSection)splitLine.Sections[sectionIdx].Clone();
                AssSection rubySection = GetRubySection(splitLine, sectionIdx);
                if (rubySection != null)
                {
                    rubySection = (AssSection)rubySection.Clone();
                    rubySection.Scale /= 2;
                    AppendBaseAndRubySections(baseSection, rubySection);
                    sectionIdx += 4;
                }
                else
                {
                    AppendBaseSection(baseSection);
                    sectionIdx++;
                }
            }

            return new SplitLineVisualization(_vizLines, _vizRight, _vizTop, _vizBottom);
        }

        private static AssSection GetRubySection(AssLine splitLine, int baseSectionIdx)
        {
            if (splitLine.Sections[baseSectionIdx].RubyPart == RubyPart.Base &&
                baseSectionIdx < splitLine.Sections.Count + 3 &&
                splitLine.Sections[baseSectionIdx + 1].RubyPart == RubyPart.Parenthesis &&
                splitLine.Sections[baseSectionIdx + 2].RubyPart is RubyPart.TextBefore or RubyPart.TextAfter &&
                splitLine.Sections[baseSectionIdx + 3].RubyPart == RubyPart.Parenthesis)
            {
                return (AssSection)splitLine.Sections[baseSectionIdx + 2];
            }
            return null;
        }

        private void AppendBaseSection(AssSection section)
        {
            SizeF size = MeasureSection(section);
            AddBaseSection(section, size);
            _vizTop = Math.Min(-size.Height, _vizTop);
            _vizRight += size.Width;
        }

        private void AddBaseSection(AssSection section, SizeF sectionSize)
        {
            if (_lastBaseVizLine == null)
            {
                _lastBaseVizLine = new(_inputLine, false) { AnchorPoint = AnchorPoint.BottomLeft, Position = new PointF(_vizRight, 0) };
                _vizLines.Add(_lastBaseVizLine);
            }
            section.RubyPart = RubyPart.None;
            _lastBaseVizLine.Sections.Add(section);
        }

        private void AppendBaseAndRubySections(AssSection baseSection, AssSection rubySection)
        {
            SizeF baseSize = MeasureSection(baseSection);
            SizeF rubySize = MeasureSection(rubySection);
            float maxWidth = Math.Max(baseSize.Width, rubySize.Width);
            if (rubySize.Width <= baseSize.Width)
            {
                AddBaseSection(baseSection, baseSize);
            }
            else
            {
                _lastBaseVizLine = null;
                AddCenteredVisualizationLine(baseSection, maxWidth, baseSize.Width, 0);
            }

            float rubyBottom;
            if (rubySection.RubyPart == RubyPart.TextBefore)
            {
                rubyBottom = -baseSize.Height;
                _vizTop = Math.Min(rubyBottom - rubySize.Height, _vizTop);
            }
            else
            {
                rubyBottom = rubySize.Height;
                _vizBottom = Math.Max(rubyBottom, _vizBottom);
            }
            rubySection.RubyPart = RubyPart.None;
            AddCenteredVisualizationLine(rubySection, maxWidth, rubySize.Width, rubyBottom);
            _vizRight += maxWidth;
        }

        private void AddCenteredVisualizationLine(AssSection section, float containingWidth, float sectionWidth, float bottom)
        {
            PointF position = new(_vizRight + containingWidth / 2 - sectionWidth / 2, bottom);
            AssLine line = new(_inputLine, false) { AnchorPoint = AnchorPoint.BottomLeft, Position = position };
            section.RubyPart = RubyPart.None;
            line.Sections.Add(section);
            _vizLines.Add(line);
        }

        private void FixPositions(List<SplitLineVisualization> visualizations)
        {
            foreach (SplitLineVisualization visualization in visualizations)
            {
                FixHorizontalCoords(visualization);
            }
            FixVerticalCoords(visualizations);
        }

        private void FixHorizontalCoords(SplitLineVisualization visualization)
        {
            float offset;
            if (AnchorPointUtil.IsLeftAligned(_inputLine.AnchorPoint))
                offset = _inputLine.Position.Value.X;
            else if (AnchorPointUtil.IsCenterAligned(_inputLine.AnchorPoint))
                offset = _inputLine.Position.Value.X - visualization.Width / 2;
            else
                offset = _inputLine.Position.Value.X - visualization.Width;

            foreach (AssLine line in visualization.Lines)
            {
                line.Position = new PointF(line.Position.Value.X + offset, line.Position.Value.Y);
            }
        }

        private void FixVerticalCoords(List<SplitLineVisualization> visualizations)
        {
            float totalHeight = visualizations.Sum(v => v.Bottom - v.Top);
            float offset;
            if (AnchorPointUtil.IsTopAligned(_inputLine.AnchorPoint))
                offset = _inputLine.Position.Value.Y;
            else if (AnchorPointUtil.IsMiddleAligned(_inputLine.AnchorPoint))
                offset = _inputLine.Position.Value.Y - totalHeight / 2;
            else
                offset = _inputLine.Position.Value.Y - totalHeight;

            foreach (SplitLineVisualization visualization in visualizations)
            {
                offset += -visualization.Top;
                foreach (AssLine line in visualization.Lines)
                {
                    line.Position = new PointF(line.Position.Value.X, line.Position.Value.Y + offset);
                }
                offset += visualization.Bottom;
            }
        }

        private SizeF MeasureSection(Section section)
        {
            if (string.IsNullOrEmpty(section.Text))
                return new SizeF(0, section.Scale * _defaultLineHeight);

            float factor;
            if (!_sizeFactors.TryGetValue(section.Font, out factor))
            {
                SizeF calibrationSize = _textMeasurer.Measure("Mgあ", section.Font, 120, false, false);
                factor = _defaultLineHeight / calibrationSize.Height;
                _sizeFactors.Add(section.Font, factor);
            }

            SizeF size = _textMeasurer.Measure(section.Text, section.Font, 120, section.Bold, section.Italic);
            return new SizeF(size.Width * factor * section.Scale, size.Height * factor * section.Scale);
        }
    }
}
