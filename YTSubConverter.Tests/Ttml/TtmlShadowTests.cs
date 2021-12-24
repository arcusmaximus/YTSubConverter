using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlShadowTests
    {
        [TestCase("20em 1c", 20, TtmlUnit.Em, 1, TtmlUnit.Cell, 0, TtmlUnit.Pixels, 0, 0, 0, 0)]
        [TestCase("20em 1c #123456", 20, TtmlUnit.Em, 1, TtmlUnit.Cell, 0, TtmlUnit.Pixels, 0x12, 0x34, 0x56, 0xFF)]
        [TestCase("20em 1c 15%", 20, TtmlUnit.Em, 1, TtmlUnit.Cell, 15, TtmlUnit.Percent, 0, 0, 0, 0)]
        [TestCase("20em   1c  15%   #12345678", 20, TtmlUnit.Em, 1, TtmlUnit.Cell, 15, TtmlUnit.Percent, 0x12, 0x34, 0x56, 0x78)]
        public void TestParseValid(
            string input,
            float expectedXValue, TtmlUnit expectedXUnit,
            float expectedYValue, TtmlUnit expectedYUnit,
            float expectedBlurValue, TtmlUnit expectedBlurUnit,
            int expectedR, int expectedG, int expectedB, int expectedA)
        {
            Assert.IsTrue(TtmlShadow.TryParse(input, out TtmlShadow actual));
            VerifyExpectations();

            actual = TtmlShadow.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedXValue, actual.Offset.Width.Value);
                Assert.AreEqual(expectedXUnit, actual.Offset.Width.Unit);
                
                Assert.AreEqual(expectedYValue, actual.Offset.Height.Value);
                Assert.AreEqual(expectedYUnit, actual.Offset.Height.Unit);

                Assert.AreEqual(expectedBlurValue, actual.BlurRadius.Value);
                Assert.AreEqual(expectedBlurUnit, actual.BlurRadius.Unit);

                Assert.AreEqual(expectedR, actual.Color.R);
                Assert.AreEqual(expectedG, actual.Color.G);
                Assert.AreEqual(expectedB, actual.Color.B);
                Assert.AreEqual(expectedA, actual.Color.A);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlShadow.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlShadow.Parse(null));
        }

        [TestCase("")]
        [TestCase("1px")]
        [TestCase("1px #123456")]
        [TestCase("1px 1px 1px #123456 1px")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlShadow.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlShadow.Parse(input));
        }
    }
}
