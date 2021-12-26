using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using YTSubConverter.Shared.Formats;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlIntegrationTests : IntegrationTestsBase
    {
        [TestCaseSource(nameof(GetForwardConversionTestCases))]
        public void TestForwardConversion(string ttmlFilePath, string expectedYttFilePath)
        {
            SubtitleDocument ttmlDoc = new TtmlDocument(ttmlFilePath);
            SubtitleDocument yttDoc = new YttDocument(ttmlDoc);

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
            SubtitleDocument ttmlDoc = new TtmlDocument(yttDoc);

            try
            {
                ttmlDoc.Save("actual.xml");

                ttmlDoc = new TtmlDocument("actual.xml");
                yttDoc = new YttDocument(ttmlDoc);
                yttDoc.Save("actual.ytt");

                string actual = File.ReadAllText("actual.ytt");
                string expected = File.ReadAllText(inputYttFilePath);
                Assert.That(actual, Is.EqualTo(expected));
            }
            finally
            {
                File.Delete("actual.xml");
                File.Delete("actual.ytt");
            }
        }

        private static IEnumerable<TestCaseData> GetForwardConversionTestCases()
        {
            foreach (string ttmlFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Ttml\\Files"), "*.xml"))
            {
                string name = Path.GetFileNameWithoutExtension(ttmlFilePath) + " (Forward)";
                string yttFilePath = Path.ChangeExtension(ttmlFilePath, ".ytt");
                yield return new TestCaseData(ttmlFilePath, yttFilePath).SetName(name);
            }
        }

        private static IEnumerable<TestCaseData> GetReverseConversionTestCases()
        {
            foreach (string yttFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Ttml\\Files"), "*.ytt"))
            {
                string name = Path.GetFileNameWithoutExtension(yttFilePath) + " (Reverse)";
                yield return new TestCaseData(yttFilePath).SetName(name);
            }
        }
    }
}
