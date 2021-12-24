using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlLengthTests
    {
        [TestCase("1px", 1f, TtmlUnit.Pixels)]
        [TestCase("1.5em", 1.5f, TtmlUnit.Em)]
        [TestCase("-20c", -20f, TtmlUnit.Cell)]
        [TestCase("9001%", 9001f, TtmlUnit.Percent)]
        [TestCase("42.rw", 42f, TtmlUnit.RootWidth)]
        [TestCase("+.42rh", .42f, TtmlUnit.RootHeight)]
        public void TestParseValid(string input, float expectedValue, TtmlUnit expectedUnit)
        {
            Assert.IsTrue(TtmlLength.TryParse(input, out TtmlLength actual));
            VerifyExpectations();

            actual = TtmlLength.Parse(input);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedValue, actual.Value);
                Assert.AreEqual(expectedUnit, actual.Unit);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlLength.TryParse(null, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlLength.Parse(null));
        }

        [TestCase("")]
        [TestCase("0")]
        [TestCase("1xp")]
        [TestCase("1 px")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlLength.TryParse(input, out _));
            Assert.Throws<FormatException>(() => TtmlLength.Parse(input));
        }
    }
}
