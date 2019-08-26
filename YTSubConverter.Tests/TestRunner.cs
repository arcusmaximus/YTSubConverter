using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Arc.YTSubConverter;
using Arc.YTSubConverter.Formats;
using Arc.YTSubConverter.Formats.Ass;
using NUnit.Framework;

namespace YTSubConverter.Tests
{
    [TestFixture]
    public class TestRunner
    {
        [TestCaseSource(nameof(GetTestCases))]
        public void Run(string assFilePath, List<AssStyleOptions> options, string expectedYttFilePath)
        {
            SubtitleDocument assDoc = new AssDocument(assFilePath, options);
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

        private static IEnumerable GetTestCases()
        {
            string baseFolderPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            List<AssStyleOptions> options = AssStyleOptionsList.Load(Path.Combine(baseFolderPath, "StyleOptions.xml"));
            foreach (string assFilePath in Directory.EnumerateFiles(Path.Combine(baseFolderPath, "Files"), "*.ass"))
            {
                string name = Path.GetFileNameWithoutExtension(assFilePath);
                string yttFilePath = Path.ChangeExtension(assFilePath, ".ytt");
                yield return new TestCaseData(assFilePath, options, yttFilePath).SetName(name);
            }
        }
    }
}
