using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter
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
                while (true)
                {
                    string timestamps = reader.ReadLine();
                    if (timestamps == null)
                        break;

                    if (string.IsNullOrWhiteSpace(timestamps))
                        continue;

                    string content = null;
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            break;

                        if (content != null)
                            content += "\r\n";

                        content += line;
                    }

                    (DateTime start, DateTime end) = ParseTimestamps(timestamps);
                    Lines.Add(new Line(start, end, content));
                }
            }
        }

        private static (DateTime, DateTime) ParseTimestamps(string timestamps)
        {
            Match match = Regex.Match(timestamps, @"^(\d+):(\d+):(\d+)\.(\d+),(\d+):(\d+):(\d+)\.(\d+)");
            return (
                new DateTime(
                    2000,
                    1,
                    1,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value)
                ),
                new DateTime(
                    2000,
                    1,
                    1,
                    int.Parse(match.Groups[5].Value),
                    int.Parse(match.Groups[6].Value),
                    int.Parse(match.Groups[7].Value),
                    int.Parse(match.Groups[8].Value)
                )
            );
        }
    }
}
