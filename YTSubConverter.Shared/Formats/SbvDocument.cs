using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats
{
    public class SbvDocument : SubtitleDocument
    {
        public SbvDocument()
        {
        }

        public SbvDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public SbvDocument(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);
            Load(reader);
        }

        public SbvDocument(Stream stream)
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            Load(reader);
        }

        public SbvDocument(TextReader reader)
        {
            Load(reader);
        }

        private void Load(TextReader reader)
        {
            string fileLine;
            Line subLine = null;
            while ((fileLine = reader.ReadLine()) != null)
            {
                if (TryParseTimestamps(fileLine, out DateTime startTime, out DateTime endTime))
                {
                    AddLine(subLine);
                    subLine = new Line(startTime, endTime);
                }
                else
                {
                    AppendToLine(subLine, fileLine);
                }
            }

            AddLine(subLine);
        }

        private static void AppendToLine(Line line, string text)
        {
            if (line == null || text == null)
                return;

            if (line.Sections.Count == 0)
                line.Sections.Add(new Section(text) { ForeColor = Color.White, BackColor = Color.FromArgb(192, 8, 8, 8) });
            else
                line.Sections[0].Text += "\r\n" + text;
        }

        private void AddLine(Line line)
        {
            if (line == null || line.Sections.Count == 0)
                return;

            line.Sections[0].Text = line.Sections[0].Text.TrimEnd();
            Lines.Add(line);
        }

        private static bool TryParseTimestamps(string timestamps, out DateTime startTime, out DateTime endTime)
        {
            Match match = Regex.Match(timestamps, @"^(\d+):(\d+):(\d+)[\.,](\d+),(\d+):(\d+):(\d+)[\.,](\d+)");
            if (!match.Success)
            {
                startTime = DateTime.MinValue;
                endTime = DateTime.MinValue;
                return false;
            }

            startTime =
                new DateTime(
                    TimeBase.Year,
                    TimeBase.Month,
                    TimeBase.Day,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value.PadRight(3, '0'))
                );
            endTime =
                new DateTime(
                    TimeBase.Year,
                    TimeBase.Month,
                    TimeBase.Day,
                    int.Parse(match.Groups[5].Value),
                    int.Parse(match.Groups[6].Value),
                    int.Parse(match.Groups[7].Value),
                    int.Parse(match.Groups[8].Value.PadRight(3, '0'))
                );
            return true;
        }
    }
}
