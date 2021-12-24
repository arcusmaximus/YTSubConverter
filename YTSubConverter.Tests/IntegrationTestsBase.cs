using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Tests
{
    public abstract class IntegrationTestsBase
    {
        protected static string DllFolderPath => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);

        /// <summary>
        /// Rounds timestamps in the expected YTT file to the center of the frame for comparing against the round-tripped actual YTT file.
        /// (The milliseconds part is lost during the conversion to .ass)
        /// </summary>
        protected static string RoundYttTimestamps(string xml)
        {
            return Regex.Replace(
                xml,
                @"<p t=""(\d+)"" d=""(\d+)""",
                m =>
                {
                    int oldStart = int.Parse(m.Groups[1].Value);
                    int oldDuration = int.Parse(m.Groups[2].Value);
                    int oldEnd = oldStart + oldDuration;

                    // Special case: don't round t="1" as this is an Android workaround from the YttDocument class    
                    int newStart = oldStart <= 1 ? oldStart : RoundTimeToFrameCenter(oldStart);
                    int newEnd = RoundTimeToFrameCenter(oldEnd);
                    int newDuration = newEnd - newStart;

                    return $@"<p t=""{newStart}"" d=""{newDuration}""";
                }
            );
        }

        protected static int RoundTimeToFrameCenter(int ms)
        {
            DateTime timestamp = SubtitleDocument.TimeBase + TimeSpan.FromMilliseconds(ms);
            return (int)(TimeUtil.RoundTimeToFrameCenter(timestamp) - SubtitleDocument.TimeBase).TotalMilliseconds;
        }
    }
}
