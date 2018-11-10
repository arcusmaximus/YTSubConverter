using System;
using System.Collections.Generic;
using System.IO;

namespace Arc.YTSubConverter
{
    internal abstract class SubtitleDocument
    {
        public List<Line> Lines { get; } = new List<Line>();

        protected SubtitleDocument()
        {
        }

        protected SubtitleDocument(SubtitleDocument doc)
        {
            Lines.AddRange(doc.Lines);
        }

        public static SubtitleDocument Load(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".ass":
                    return new AssDocument(filePath);

                case ".sbv":
                    return new SbvDocument(filePath);

                case ".srt":
                    return new SrtDocument(filePath);

                default:
                    throw new NotSupportedException();
            }
        }

        public void CloseGaps()
        {
            TimeSpan threshold = new TimeSpan(0, 0, 0, 0, 50);
            for (int i = 0; i < Lines.Count - 1; i++)
            {
                if (Lines[i + 1].Start - Lines[i].End < threshold)
                    Lines[i].End = Lines[i + 1].Start;
            }
        }

        public void Shift(TimeSpan offset)
        {
            foreach (Line line in Lines)
            {
                line.Start += offset;
                line.End += offset;
            }

            if (Lines.Count > 0 && Lines[0].Start.Year < 2000)
                Lines[0].Start = new DateTime(2000, 1, 1);
        }

        public virtual void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
