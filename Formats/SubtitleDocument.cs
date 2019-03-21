using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter.Formats
{
    internal abstract class SubtitleDocument
    {
        public static readonly DateTime TimeBase = new DateTime(2000, 1, 1);

        protected SubtitleDocument()
        {
        }

        protected SubtitleDocument(SubtitleDocument doc)
        {
            VideoDimensions = doc.VideoDimensions;
            Lines.AddRange(doc.Lines);
        }

        public Size VideoDimensions
        {
            get;
            set;
        }

        public List<Line> Lines { get; } = new List<Line>();

        public static SubtitleDocument Load(string filePath)
        {
            switch (Path.GetExtension(filePath)?.ToLower())
            {
                case ".ass":
                    return new AssDocument(filePath);

                case ".sbv":
                    return new SbvDocument(filePath);

                case ".srt":
                    return new SrtDocument(filePath);

                case ".ytt":
                    return new YttDocument(filePath);

                default:
                    throw new NotSupportedException();
            }
        }

        public void Compact()
        {
            SortedList<DateTime, List<Line>> linesByStartTime = new SortedList<DateTime, List<Line>>();
            foreach (Line line in Lines)
            {
                linesByStartTime.FetchValue(line.Start, () => new List<Line>()).Add(line);
            }

            IEnumerable<Line> linesToCheck = Lines;
            List<Line> changedLines;
            HashSet<Line> linesToRemove;

            do
            {
                changedLines = new List<Line>();
                linesToRemove = new HashSet<Line>();

                foreach (Line line in linesToCheck)
                {
                    if (linesToRemove.Contains(line))
                        continue;

                    int endTimeIdx = linesByStartTime.Keys.BinarySearchIndexAtOrAfter(line.End);

                    int timeGapBefore = endTimeIdx > 0 ? (int)(line.End - linesByStartTime.Keys[endTimeIdx - 1]).TotalMilliseconds : int.MaxValue;
                    int timeGapAfter = endTimeIdx < linesByStartTime.Count ? (int)(linesByStartTime.Keys[endTimeIdx] - line.End).TotalMilliseconds : int.MaxValue;

                    if (timeGapBefore < 50 && timeGapBefore < timeGapAfter)
                        endTimeIdx--;
                    else if (timeGapAfter < 50 && timeGapAfter <= timeGapBefore)
                        ;
                    else
                        continue;

                    line.End = linesByStartTime.Keys[endTimeIdx];

                    /*
                    List<Line> linesStartingAtEndTime = linesByStartTime.Values[endTimeIdx];
                    Line followingLine = linesStartingAtEndTime.FirstOrDefault(l => AreLineContentsAndFormatsEqual(line, l));
                    if (followingLine != null)
                    {
                        linesToRemove.Add(followingLine);
                        linesStartingAtEndTime.Remove(followingLine);
                        if (linesStartingAtEndTime.Count == 0)
                            linesByStartTime.Remove(followingLine.Start);

                        line.End = followingLine.End;
                        changedLines.Add(line);
                    }
                    */
                }

                foreach (Line line in linesToRemove)
                {
                    Lines.Remove(line);
                    changedLines.Remove(line);
                }

                linesToCheck = changedLines;
            } while (changedLines.Count > 0);
        }

        private static bool AreLineContentsAndFormatsEqual(Line line1, Line line2)
        {
            if (line1.AnchorPoint != line2.AnchorPoint || line1.Position != line2.Position || line1.Sections.Count != line2.Sections.Count)
                return false;

            SectionFormatComparer formatComparer = new SectionFormatComparer();
            for (int i = 0; i < line1.Sections.Count; i++)
            {
                Section section1 = line1.Sections[i];
                Section section2 = line2.Sections[i];
                if (section1.Text != section2.Text || !formatComparer.Equals(section1, section2))
                    return false;
            }

            return true;
        }

        public virtual void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
