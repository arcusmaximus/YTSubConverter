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

        public void CloseGaps()
        {
            SortedList<DateTime, List<Line>> linesByStartTime = new SortedList<DateTime, List<Line>>();
            foreach (Line line in Lines)
            {
                linesByStartTime.FetchValue(line.Start, () => new List<Line>()).Add(line);
            }

            foreach (Line line in Lines)
            {
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
            }
        }

        public virtual void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
