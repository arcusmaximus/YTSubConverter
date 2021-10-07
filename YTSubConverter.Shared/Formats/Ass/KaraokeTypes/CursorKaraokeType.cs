using System;
using System.Collections.Generic;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared.Formats.Ass.KaraokeTypes
{
    public class CursorKaraokeType : SimpleKaraokeType
    {
        private readonly IList<string> _cursors;
        private readonly int _intervalMs;
        private readonly bool _beforeSinging;

        public CursorKaraokeType(IList<string> cursors, TimeSpan interval, bool beforeSinging)
        {
            _cursors = cursors;
            _intervalMs = (int)interval.TotalMilliseconds;
            _beforeSinging = beforeSinging;
        }

        public override IEnumerable<AssLine> Apply(AssKaraokeStepContext context)
        {
            base.Apply(context);

            int startCursorStepIdx = (int)(context.StepLine.Start - context.OriginalLine.Start).TotalMilliseconds / _intervalMs;
            int endCursorStepIdx = (int)(context.StepLine.End - context.OriginalLine.Start).TotalMilliseconds / _intervalMs;
            for (int cursorStepIdx = startCursorStepIdx; cursorStepIdx <= endCursorStepIdx; cursorStepIdx++)
            {
                AssLine cursorStepLine = (AssLine)context.StepLine.Clone();
                if (cursorStepIdx > startCursorStepIdx)
                    cursorStepLine.Start = TimeUtil.RoundTimeToFrameCenter(context.OriginalLine.Start.AddMilliseconds(cursorStepIdx * _intervalMs));

                if (cursorStepIdx < endCursorStepIdx)
                    cursorStepLine.End = TimeUtil.RoundTimeToFrameCenter(context.OriginalLine.Start.AddMilliseconds((cursorStepIdx + 1) * _intervalMs));

                if (cursorStepLine.Start == cursorStepLine.End)
                    continue;

                int cursorSectionIdx = _beforeSinging ? context.NumActiveSections - context.SingingSections.Count : context.NumActiveSections;
                AssSection initialFormatting = (AssSection)cursorStepLine.Sections[Math.Max(cursorSectionIdx - 1, 0)];
                List<Section> cursorSections = GenerateCursor(context.Document, initialFormatting, _cursors[cursorStepIdx % _cursors.Count]);
                cursorStepLine.Sections.InsertRange(cursorSectionIdx, cursorSections);

                yield return cursorStepLine;
            }
        }

        private List<Section> GenerateCursor(AssDocument doc, AssSection initialFormatting, string cursor)
        {
            AssSection section = (AssSection)initialFormatting.Clone();
            section.Text = string.Empty;

            AssLine line = new AssLine(SubtitleDocument.TimeBase, SubtitleDocument.TimeBase);
            AssTagContext context =
                new AssTagContext
                {
                    Document = doc,
                    Line = line,
                    Section = section
                };
            doc.CreateTagSections(line, cursor, context);

            return line.Sections;
        }
    }
}
