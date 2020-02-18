using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Formats
{
    internal class SbvDocument : SubtitleDocument
    {
        public SbvDocument()
        {
        }

        public SbvDocument(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string fileLine;
                Line subLine = null;
                while ((fileLine = reader.ReadLine()) != null)
                {
                    if (TryParseTimestamps(fileLine, out DateTime startTime, out DateTime endTime))
                    {
                        if (subLine != null && subLine.Sections.Count > 0)
                        {
                            subLine.Sections[0].Text = subLine.Sections[0].Text.TrimEnd();
                            Lines.Add(subLine);
                        }

                        subLine = new Line(startTime, endTime);
                    }
                    else if (subLine != null)
                    {
                        if (subLine.Sections.Count == 0)
                            subLine.Sections.Add(new Section(fileLine));
                        else
                            subLine.Sections[0].Text += "\r\n" + fileLine;
                    }
                }

                if (subLine != null && subLine.Sections.Count > 0)
                {
                    subLine.Sections[0].Text = subLine.Sections[0].Text.TrimEnd();
                    Lines.Add(subLine);
                }
            }
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
                    int.Parse(match.Groups[4].Value)
                );
            endTime =
                new DateTime(
                    TimeBase.Year,
                    TimeBase.Month,
                    TimeBase.Day,
                    int.Parse(match.Groups[5].Value),
                    int.Parse(match.Groups[6].Value),
                    int.Parse(match.Groups[7].Value),
                    int.Parse(match.Groups[8].Value)
                );
            return true;
        }
    }
}
