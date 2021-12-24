using System;
using System.Drawing;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlColorTests
    {
        [TestCase("transparent", 0, 0, 0, 0)]
        [TestCase("red", 255, 0, 0, 255)]
        [TestCase("#123A5F", 0x12, 0x3A, 0x5F, 255)]
        [TestCase("#123A5B6C", 0x12, 0x3A, 0x5B, 0x6C)]
        [TestCase("rgb(12,34,56)", 12, 34, 56, 255)]
        [TestCase("rgb( 12 , 34 , 56 )", 12, 34, 56, 255)]
        [TestCase("rgba(12,34,56,78)", 12, 34, 56, 78)]
        [TestCase("rgba( 12 , 34 , 56 , 78 )", 12, 34, 56, 78)]
        public void TestParseValid(string input, int expectedR, int expectedG, int expectedB, int expectedA)
        {
            Assert.IsTrue(TtmlColor.TryParse(input, out Color actual));
            VerifyExpectations();

            actual = TtmlColor.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedR, actual.R);
                Assert.AreEqual(expectedG, actual.G);
                Assert.AreEqual(expectedB, actual.B);
                Assert.AreEqual(expectedA, actual.A);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlColor.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlColor.Parse(null));
        }

        [TestCase("")]
        [TestCase("invalid")]
        [TestCase("123")]
        [TestCase("#123")]
        [TestCase("#1234XX")]
        [TestCase("#123456XX")]
        [TestCase("rgb(12, 34)")]
        [TestCase("rgb(12, 35, 56")]
        [TestCase("rgb(12, 34, 56, 78)")]
        [TestCase("rgb(0, 0, 256)")]
        [TestCase("rgb(-1, 0, 0)")]
        [TestCase("rgb(A, B, C)")]
        [TestCase("rgba(12, 34, 56)")]
        [TestCase("rgba(12, 34, 56, 78, 90)")]
        [TestCase("rgbx(1, 2, 3, 4)")]
        [TestCase("x(1, 2, 3)")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlColor.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlColor.Parse(input));
        }
    }
}
