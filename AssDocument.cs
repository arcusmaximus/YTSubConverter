using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter
{
    internal class AssDocument : SubtitleDocument
    {
        public AssDocument(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Match match = Regex.Match(line, @"^Dialogue: \d+,(\d+):(\d+):(\d+)\.(\d+),(\d+):(\d+):(\d+)\.(\d+),.*?,.*?,\d+,\d+,\d+,.*?,(.*)");
                    if (!match.Success)
                        continue;

                    DateTime start = CreateTimestamp(
                        int.Parse(match.Groups[1].Value),
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value),
                        true
                    );

                    DateTime end = CreateTimestamp(
                        int.Parse(match.Groups[5].Value),
                        int.Parse(match.Groups[6].Value),
                        int.Parse(match.Groups[7].Value),
                        int.Parse(match.Groups[8].Value),
                        false
                    );

                    string text = match.Groups[9].Value;

                    Lines.Add(new Line(start, end, ParseText(text)));
                }
            }
        }

        private static DateTime CreateTimestamp(int hours, int minutes, int seconds, int centiseconds, bool isStart)
        {
            int totalMs = (((hours * 60) + minutes) * 60 + seconds) * 1000 + (centiseconds * 10);
            int frameNr = (int)(totalMs / 33.36666666666667);
            if (frameNr == 0)
                return new DateTime(2000, 1, 1, 0, 0, 0, 0);

            if (isStart)
                frameNr++;

            totalMs = (int)(frameNr * 33.36666666666667);
            if (isStart)
                totalMs--;

            int ms;
            int totalSeconds = Math.DivRem(totalMs, 1000, out ms);
            int totalMinutes = Math.DivRem(totalSeconds, 60, out seconds);
            hours = Math.DivRem(totalMinutes, 60, out minutes);
            return new DateTime(2000, 1, 1, hours, minutes, seconds, ms);
        }

        private static IEnumerable<Section> ParseText(string text)
        {
            text = Regex.Replace(text, @"(?:\\N)+$", "");

            bool bold = false;
            bool italic = false;
            bool underline = false;
            Color color = Color.Empty;

            int start = 0;
            foreach (Match match in Regex.Matches(text, @"\{(?:\\(?<cmd>[a-z]+)(?<arg>.*?))+\}"))
            {
                int end = match.Index;

                if (end > start)
                    yield return CreateSection(text, start, end, bold, italic, underline, color);

                CaptureCollection commands = match.Groups["cmd"].Captures;
                CaptureCollection arguments = match.Groups["arg"].Captures;
                for (int i = 0; i < commands.Count; i++)
                {
                    switch (commands[i].Value)
                    {
                        case "b":
                            bold = arguments[i].Value != "0";
                            break;

                        case "i":
                            italic = arguments[i].Value != "0";
                            break;

                        case "u":
                            underline = arguments[i].Value != "0";
                            break;

                        case "c":
                            int r = int.Parse(arguments[i].Value.Substring(6, 2), NumberStyles.AllowHexSpecifier);
                            int g = int.Parse(arguments[i].Value.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                            int b = int.Parse(arguments[i].Value.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                            if (r == 255 && b == 255 && g == 255)
                                color = Color.Empty;
                            else
                                color = Color.FromArgb(r, g, b);

                            break;
                    }
                }

                start = match.Index + match.Length;
            }

            if (start < text.Length)
                yield return CreateSection(text, start, text.Length, bold, italic, underline, color);
        }

        private static Section CreateSection(string text, int start, int end, bool bold, bool italic, bool underline, Color color)
        {
            string sectionText = text.Substring(start, end - start).Replace("\\N", "\r\n");
            return new Section(sectionText) { Bold = bold, Italic = italic, Underline = underline, Color = color };
        }
    }
}
