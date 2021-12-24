using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats.Ttml
{
    public static class TtmlTime
    {
        public static bool TryParse(string text, float frameRate, float subFrameRate, float tickRate, out DateTime value)
        {
            if (string.IsNullOrEmpty(text))
            {
                value = new DateTime();
                return false;
            }

            return TryParseClockTime(text, frameRate, subFrameRate, out value) ||
                   TryParseOffsetTime(text, frameRate, tickRate, out value);
        }

        public static DateTime Parse(string text, float frameRate, float subFrameRate, float tickRate)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!TryParse(text, frameRate, subFrameRate, tickRate, out DateTime value))
                throw new FormatException();

            return value;
        }

        private static bool TryParseClockTime(string text, float frameRate, float subFrameRate, out DateTime value)
        {
            Match match = Regex.Match(text, @"^(?<hours>\d+):(?<minutes>\d+):(?<seconds>\d+)(?:\.(?<fraction>\d+)|:(?<frames>\d+)(?:\.(?<subframes>\d+))?)?$");
            if (!match.Success)
            {
                value = new DateTime();
                return false;
            }

            int ms = 0;
            if (match.Groups["fraction"].Success)
            {
                string fraction = match.Groups["fraction"].Value;
                ms = (int)Math.Round(int.Parse(fraction) / Math.Pow(10, Math.Max(fraction.Length - 3, 0)) * Math.Pow(10, Math.Max(3 - fraction.Length, 0)));
            }
            else if (match.Groups["frames"].Success)
            {
                ms = (int)(int.Parse(match.Groups["frames"].Value) / frameRate * 1000);
                if (match.Groups["subframes"].Success)
                    ms += (int)(int.Parse(match.Groups["subframes"].Value) / (frameRate * subFrameRate) * 1000);
            }

            try
            {
                value = new DateTime(
                    SubtitleDocument.TimeBase.Year,
                    SubtitleDocument.TimeBase.Month,
                    SubtitleDocument.TimeBase.Day,
                    int.Parse(match.Groups["hours"].Value),
                    int.Parse(match.Groups["minutes"].Value),
                    int.Parse(match.Groups["seconds"].Value),
                    ms
                );
            }
            catch
            {
                value = new DateTime();
                return false;
            }

            return true;
        }

        private static bool TryParseOffsetTime(string text, float frameRate, float tickRate, out DateTime value)
        {
            Match match = Regex.Match(text, @"^(?<offset>\d+(?:\.\d+)?)(?<metric>h|m|s|ms|f|t)$");
            if (!match.Success)
            {
                value = new DateTime();
                return false;
            }

            float offset = float.Parse(match.Groups["offset"].Value, CultureInfo.InvariantCulture);
            string metric = match.Groups["metric"].Value;
            Func<double, DateTime> addOffset = metric switch
                                               {
                                                   "h" => SubtitleDocument.TimeBase.AddHours,
                                                   "m" => SubtitleDocument.TimeBase.AddMinutes,
                                                   "s" => SubtitleDocument.TimeBase.AddSeconds,
                                                   "ms" => SubtitleDocument.TimeBase.AddMilliseconds,
                                                   "f" => f => SubtitleDocument.TimeBase.AddSeconds(f / frameRate),
                                                   "t" => t => SubtitleDocument.TimeBase.AddSeconds(t / tickRate),
                                                   _ => throw new FormatException()
                                               };
            value = addOffset(offset);
            return true;
        }

        public static string ToString(DateTime time)
        {
            return $"{time.Hour:d02}:{time.Minute:d02}:{time.Second:d02}.{time.Millisecond:d03}";
        }
    }
}
