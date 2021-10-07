using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Tests
{
    [TestFixture]
    public class TestRunner
    {
        [TestCaseSource(nameof(GetForwardConversionTestCases))]
        public void TestForwardConversion(string assFilePath, List<AssStyleOptions> styleOptions, string expectedYttFilePath)
        {
            SubtitleDocument assDoc = new AssDocument(assFilePath, styleOptions);
            SubtitleDocument yttDoc = new YttDocument(assDoc);

            try
            {
                yttDoc.Save("actual.ytt");

                string actual = File.ReadAllText("actual.ytt");
                string expected = File.ReadAllText(expectedYttFilePath);
                Assert.That(actual, Is.EqualTo(expected));
            }
            finally
            {
                File.Delete("actual.ytt");
            }
        }

        [TestCaseSource(nameof(GetReverseConversionTestCases))]
        public void TestReverseConversion(string inputYttFilePath)
        {
            SubtitleDocument yttDoc = new YttDocument(inputYttFilePath);
            SubtitleDocument assDoc = new AssDocument(yttDoc);

            try
            {
                assDoc.Save("actual.ass");

                assDoc = new AssDocument("actual.ass", AssStyleOptionsList.LoadFromString(Resources.DefaultStyleOptions));
                yttDoc = new YttDocument(assDoc);
                yttDoc.Save("actual.ytt");

                string actual = File.ReadAllText("actual.ytt");
                string expected = RoundYttTimestamps(File.ReadAllText(inputYttFilePath));
                Assert.That(actual, Is.EqualTo(expected));
            }
            finally
            {
                File.Delete("actual.ass");
                File.Delete("actual.ytt");
            }
        }

        private static IEnumerable GetForwardConversionTestCases()
        {
            List<AssStyleOptions> options = AssStyleOptionsList.LoadFromFile(Path.Combine(DllFolderPath, "StyleOptions.xml"));
            foreach (string assFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Files"), "*.ass"))
            {
                string name = Path.GetFileNameWithoutExtension(assFilePath) + " (Forward)";
                string yttFilePath = Path.ChangeExtension(assFilePath, ".ytt");
                yield return new TestCaseData(assFilePath, options, yttFilePath).SetName(name);
            }
        }

        private static IEnumerable GetReverseConversionTestCases()
        {
            foreach (string yttFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Files"), "*.ytt"))
            {
                string name = Path.GetFileNameWithoutExtension(yttFilePath) + " (Reverse)";
                yield return new TestCaseData(yttFilePath).SetName(name);
            }
        }

        private static string DllFolderPath => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);

        /// <summary>
        /// Rounds timestamps in the expected YTT file to the center of the frame for comparing against the round-tripped actual YTT file.
        /// (The milliseconds part is lost during the conversion to .ass)
        /// </summary>
        private static string RoundYttTimestamps(string xml)
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

        private static int RoundTimeToFrameCenter(int ms)
        {
            DateTime timestamp = SubtitleDocument.TimeBase + TimeSpan.FromMilliseconds(ms);
            return (int)(TimeUtil.RoundTimeToFrameCenter(timestamp) - SubtitleDocument.TimeBase).TotalMilliseconds;
        }
    }
}
