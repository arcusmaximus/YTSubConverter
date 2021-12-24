using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlOutlineTests
    {
        [TestCase("none", 0, 0, 0, 0, 0f, TtmlUnit.Pixels, 0f, TtmlUnit.Pixels)]
        [TestCase("1px", 0, 0, 0, 0, 1f, TtmlUnit.Pixels, 0f, TtmlUnit.Pixels)]
        [TestCase("cyan 5%", 0, 255, 255, 255, 5f, TtmlUnit.Percent, 0f, TtmlUnit.Pixels)]
        [TestCase("5px 2px", 0, 0, 0, 0, 5f, TtmlUnit.Pixels, 2f, TtmlUnit.Pixels)]
        [TestCase("rgba(1, 2, 3, 4)    10%    .1em", 1, 2, 3, 4, 10f, TtmlUnit.Percent, 0.1f, TtmlUnit.Em)]
        public void TestParseValid(
            string input,
            int expectedR, int expectedG, int expectedB, int expectedA,
            float expectedThicknessValue, TtmlUnit expectedThicknessUnit,
            float expectedBlurRadiusValue, TtmlUnit expectedBlurRadiusUnit)
        {
            Assert.IsTrue(TtmlOutline.TryParse(input, out TtmlOutline actual));
            VerifyExpectations();

            actual = TtmlOutline.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedR, actual.Color.R);
                Assert.AreEqual(expectedG, actual.Color.G);
                Assert.AreEqual(expectedB, actual.Color.B);
                Assert.AreEqual(expectedA, actual.Color.A);
                Assert.AreEqual(expectedThicknessValue, actual.Thickness.Value);
                Assert.AreEqual(expectedThicknessUnit, actual.Thickness.Unit);
                Assert.AreEqual(expectedBlurRadiusValue, actual.BlurRadius.Value);
                Assert.AreEqual(expectedBlurRadiusUnit, actual.BlurRadius.Unit);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlOutline.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlOutline.Parse(null));
        }

        [TestCase("")]
        [TestCase("black")]
        [TestCase("1px 2px 3px")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlOutline.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlOutline.Parse(input));
        }
    }
}
