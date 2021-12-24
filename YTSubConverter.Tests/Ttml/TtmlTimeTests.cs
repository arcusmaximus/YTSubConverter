using System;
using NUnit.Framework;
using YTSubConverter.Shared.Formats.Ttml;

namespace YTSubConverter.Tests.Ttml
{
    [TestFixture]
    public class TtmlTimeTests
    {
        private const int FrameRate = 25;
        private const int SubFrameRate = 2;
        private const int TickRate = 10;

        [TestCase("1:2:3", 1, 2, 3, 0)]
        [TestCase("12:13:14.1", 12, 13, 14, 100)]
        [TestCase("12:13:14.15", 12, 13, 14, 150)]
        [TestCase("12:13:14.156", 12, 13, 14, 156)]
        [TestCase("12:13:14.1567", 12, 13, 14, 157)]

        [TestCase("0:10:20:2", 0, 10, 20, 1000 / FrameRate * 2)]
        [TestCase("0:10:20:2.1", 0, 10, 20, 1000 / FrameRate * 2 + 1000 / (FrameRate * SubFrameRate) * 1)]

        [TestCase("1h", 1, 0, 0, 0)]
        [TestCase("1.5h", 1, 30, 0, 0)]
        [TestCase("1.5m", 0, 1, 30, 0)]
        [TestCase("20.5s", 0, 0, 20, 500)]
        [TestCase("100ms", 0, 0, 0, 100)]
        [TestCase("2f", 0, 0, 0, 1000 / FrameRate * 2)]
        [TestCase("2t", 0, 0, 0, 1000 / TickRate * 2)]
        public void TestParseValid(string input, int expectedHours, int expectedMinutes, int expectedSeconds, int expectedMilliseconds)
        {
            Assert.IsTrue(TtmlTime.TryParse(input, FrameRate, SubFrameRate, TickRate, out DateTime actual));
            VerifyExpectations();

            actual = TtmlTime.Parse(input, FrameRate, SubFrameRate, TickRate);
            VerifyExpectations();

            void VerifyExpectations()
            {
                Assert.AreEqual(expectedHours, actual.Hour);
                Assert.AreEqual(expectedMinutes, actual.Minute);
                Assert.AreEqual(expectedSeconds, actual.Second);
                Assert.AreEqual(expectedMilliseconds, actual.Millisecond);
            }
        }

        [Test]
        public void TestParseNull()
        {
            Assert.IsFalse(TtmlTime.TryParse(null, FrameRate, SubFrameRate, TickRate, out _));
            Assert.Throws<ArgumentNullException>(() => TtmlTime.Parse(null, FrameRate, SubFrameRate, TickRate));
        }

        [TestCase("")]
        [TestCase("1")]
        [TestCase("1:2")]
        [TestCase("1:60:00")]
        [TestCase("1:00:60")]
        [TestCase("abc")]
        public void TestParseInvalid(string input)
        {
            Assert.IsFalse(TtmlTime.TryParse(input, FrameRate, SubFrameRate, TickRate, out _));
            Assert.Throws<FormatException>(() => TtmlTime.Parse(input, FrameRate, SubFrameRate, TickRate));
        }
    }
}
