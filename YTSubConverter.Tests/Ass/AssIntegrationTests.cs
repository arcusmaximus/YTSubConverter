using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ass;

namespace YTSubConverter.Tests.Ass
{
    [TestFixture]
    public class AssIntegrationTests : IntegrationTestsBase
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

        private static IEnumerable<TestCaseData> GetForwardConversionTestCases()
        {
            List<AssStyleOptions> options = AssStyleOptionsList.LoadFromFile(Path.Combine(DllFolderPath, "StyleOptions.xml"));
            foreach (string assFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Ass\\Files"), "*.ass"))
            {
                string name = Path.GetFileNameWithoutExtension(assFilePath) + " (Forward)";
                string yttFilePath = Path.ChangeExtension(assFilePath, ".ytt");
                yield return new TestCaseData(assFilePath, options, yttFilePath).SetName(name);
            }
        }

        private static IEnumerable<TestCaseData> GetReverseConversionTestCases()
        {
            foreach (string yttFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Ass\\Files"), "*.ytt"))
            {
                string name = Path.GetFileNameWithoutExtension(yttFilePath) + " (Reverse)";
                yield return new TestCaseData(yttFilePath).SetName(name);
            }
        }
    }
}
