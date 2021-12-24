using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlPositionTests
    {
        [TestCase("left", TtmlPosHBase.Left, 0, TtmlUnit.Pixels, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("center", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("right", TtmlPosHBase.Right, 0, TtmlUnit.Pixels, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("top", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 0, TtmlUnit.Pixels)]
        [TestCase("bottom", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Bottom, 0, TtmlUnit.Pixels)]
        [TestCase("20%", TtmlPosHBase.Left, 20, TtmlUnit.Percent, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]

        [TestCase("right top", TtmlPosHBase.Right, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 0, TtmlUnit.Pixels)]
        [TestCase("center left", TtmlPosHBase.Left, 0, TtmlUnit.Pixels, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("center center", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("center top", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 0, TtmlUnit.Pixels)]
        [TestCase("bottom center", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Bottom, 0, TtmlUnit.Pixels)]
        [TestCase("bottom left", TtmlPosHBase.Left, 0, TtmlUnit.Pixels, TtmlPosVBase.Bottom, 0, TtmlUnit.Pixels)]
        [TestCase("left 20%", TtmlPosHBase.Left, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 20, TtmlUnit.Percent)]
        [TestCase("center 3em", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 3, TtmlUnit.Em)]
        [TestCase("3em center", TtmlPosHBase.Left, 3, TtmlUnit.Em, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("20% top", TtmlPosHBase.Left, 20, TtmlUnit.Percent, TtmlPosVBase.Top, 0, TtmlUnit.Pixels)]
        [TestCase("100px 60em", TtmlPosHBase.Left, 100, TtmlUnit.Pixels, TtmlPosVBase.Top, 60, TtmlUnit.Em)]

        [TestCase("right 20% top", TtmlPosHBase.Right, 20, TtmlUnit.Percent, TtmlPosVBase.Top, 0, TtmlUnit.Pixels)]
        [TestCase("right top 20%", TtmlPosHBase.Right, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 20, TtmlUnit.Percent)]
        [TestCase("center left 20%", TtmlPosHBase.Left, 20, TtmlUnit.Percent, TtmlPosVBase.Center, 0, TtmlUnit.Pixels)]
        [TestCase("center top 20%", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Top, 20, TtmlUnit.Percent)]
        [TestCase("bottom 20% center", TtmlPosHBase.Center, 0, TtmlUnit.Pixels, TtmlPosVBase.Bottom, 20, TtmlUnit.Percent)]
        [TestCase("bottom left 20%", TtmlPosHBase.Left, 20, TtmlUnit.Percent, TtmlPosVBase.Bottom, 0, TtmlUnit.Pixels)]

        [TestCase("left 100c bottom 3em", TtmlPosHBase.Left, 100, TtmlUnit.Cell, TtmlPosVBase.Bottom, 3, TtmlUnit.Em)]
        [TestCase("top   3em   right    100c", TtmlPosHBase.Right, 100, TtmlUnit.Cell, TtmlPosVBase.Top, 3, TtmlUnit.Em)]
        public void TestParseValid(
            string input,
            TtmlPosHBase expectedHBase, float expectedHOffsetValue, TtmlUnit expectedHOffsetUnit,
            TtmlPosVBase expectedVBase, float expectedVOffsetValue, TtmlUnit expectedVOffsetUnit)
        {
            Assert.IsTrue(TtmlPosition.TryParse(input, out TtmlPosition actual));
            VerifyExpectations();

            actual = TtmlPosition.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedHBase, actual.HBase);
                Assert.AreEqual(expectedHOffsetValue, actual.Offset.Width.Value);
                Assert.AreEqual(expectedHOffsetUnit, actual.Offset.Width.Unit);

                Assert.AreEqual(expectedVBase, actual.VBase);
                Assert.AreEqual(expectedVOffsetValue, actual.Offset.Height.Value);
                Assert.AreEqual(expectedVOffsetUnit, actual.Offset.Height.Unit);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlPosition.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlPosition.Parse(null));
        }

        [TestCase("")]
        [TestCase("invalid")]
        [TestCase("center 20% center")]
        [TestCase("left top left")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlPosition.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlPosition.Parse(input));
        }
    }
}
