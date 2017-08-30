using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction.Time
{
    [TestClass]
    public class MetricTimeTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse metric time in form of 'Hours:Minutes:Seconds:Milliseconds'.")]
        public void TryParse_HoursMinutesSecondsMilliseconds()
        {
            TestTryParse("2:4:30:567", new MetricTime(2, 4, 30, 567));
        }

        [TestMethod]
        [Description("Try parse metric time in form of 'Hours:Minutes:Seconds'.")]
        public void TryParse_HoursMinutesSeconds()
        {
            TestTryParse("2:4:30", new MetricTime(2, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric time in form of 'Minutes:Seconds'.")]
        public void TryParse_MinutesSeconds()
        {
            TestTryParse("4:30", new MetricTime(0, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric time in form of 'Milliseconds'.")]
        public void TryParse_Milliseconds()
        {
            TestTryParse("430", new MetricTime(0, 0, 0, 430));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            var expectedTime = new MetricTime(3, 6, 8, 987);
            Assert.AreEqual(expectedTime, MetricTime.Parse(expectedTime.ToString()));
        }

        #endregion

        #region Private methods

        private static void TestTryParse(string input, MetricTime expectedTime)
        {
            MetricTime.TryParse(input, out var actualTime);
            Assert.AreEqual(expectedTime, actualTime);
        }

        #endregion
    }
}
