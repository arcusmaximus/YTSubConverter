using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Formats
{
    internal class SrtDocument : SubtitleDocument
    {
        public SrtDocument()
        {
        }

        public SrtDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public SrtDocument(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);

            string fileLine;
            Line subLine = null;
            string lineNumber = null;
            while ((fileLine = reader.ReadLine()) != null)
            {
                if (int.TryParse(fileLine, out _))
                {
                    AppendToLine(subLine, lineNumber);
                    lineNumber = fileLine;
                    continue;
                }

                if (lineNumber != null && TryParseTimestamps(fileLine, out DateTime start, out DateTime end))
                {
                    AddLine(subLine);
                    subLine = new Line(start, end);
                    lineNumber = null;
                    continue;
                }

                AppendToLine(subLine, lineNumber);
                lineNumber = null;

                AppendToLine(subLine, fileLine);
            }

            AddLine(subLine);
        }

        public override void Save(string filePath)
        {
            using StreamWriter writer = new StreamWriter(filePath);

            int index = 1;
            foreach (Line line in Lines)
            {
                writer.WriteLine(index.ToString());
                writer.WriteLine(FormatTimestamps(line.Start, line.End));
                writer.WriteLine(line.Text);
                writer.WriteLine();

                index++;
            }
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

        private static bool TryParseTimestamps(string timestamps, out DateTime start, out DateTime end)
        {
            Match match = Regex.Match(timestamps, @"^(\d+):(\d+):(\d+)[\.,](\d+) --> (\d+):(\d+):(\d+)[\.,](\d+)");
            if (!match.Success)
            {
                start = default;
                end = default;
                return false;
            }

            start = new DateTime(
                TimeBase.Year,
                TimeBase.Month,
                TimeBase.Day,
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                int.Parse(match.Groups[4].Value.PadRight(3, '0'))
            );
            end = new DateTime(
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

        private static string FormatTimestamps(DateTime startTime, DateTime endTime)
        {
            return $"{startTime.Hour:00}:{startTime.Minute:00}:{startTime.Second:00},{startTime.Millisecond:000} --> " +
                   $"{endTime.Hour:00}:{endTime.Minute:00}:{endTime.Second:00},{endTime.Millisecond:000}";
        }
    }
}
