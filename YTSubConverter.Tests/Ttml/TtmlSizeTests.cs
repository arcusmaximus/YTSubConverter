using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlSizeTests
    {
        [TestCase("20px 30px", 20, TtmlUnit.Pixels, 30, TtmlUnit.Pixels)]
        [TestCase("20.5em    30c", 20.5f, TtmlUnit.Em, 30, TtmlUnit.Cell)]
        public void TestParseValid(
            string input,
            float expectedWidthValue, TtmlUnit expectedWidthUnit,
            float expectedHeightValue, TtmlUnit expectedHeightUnit)
        {
            Assert.IsTrue(TtmlSize.TryParse(input, out TtmlSize actual));
            VerifyExpectations();

            actual = TtmlSize.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedWidthValue, actual.Width.Value);
                Assert.AreEqual(expectedWidthUnit, actual.Width.Unit);

                Assert.AreEqual(expectedHeightValue, actual.Height.Value);
                Assert.AreEqual(expectedHeightUnit, actual.Height.Unit);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlSize.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlSize.Parse(null));
        }

        [TestCase("")]
        [TestCase("invalid")]
        [TestCase("1px")]
        [TestCase("1px 1px 1px")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlSize.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlSize.Parse(input));
        }
    }
}
