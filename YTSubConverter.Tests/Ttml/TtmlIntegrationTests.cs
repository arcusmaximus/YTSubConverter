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

        private static IEnumerable<TestCaseData> GetForwardConversionTestCases()
        {
            foreach (string ttmlFilePath in Directory.EnumerateFiles(Path.Combine(DllFolderPath, "Ttml\\Files"), "*.xml"))
            {
                string name = Path.GetFileNameWithoutExtension(ttmlFilePath) + " (Forward)";
                string yttFilePath = Path.ChangeExtension(ttmlFilePath, ".ytt");
                yield return new TestCaseData(ttmlFilePath, yttFilePath).SetName(name);
            }
        }
    }
}
