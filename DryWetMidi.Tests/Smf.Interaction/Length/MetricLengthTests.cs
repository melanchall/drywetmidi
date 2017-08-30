using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction.Length
{
    [TestClass]
    public class MetricLengthTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds:Milliseconds'.")]
        public void TryParse_HoursMinutesSecondsMilliseconds()
        {
            TestTryParse("2:4:30:567", new MetricLength(2, 4, 30, 567));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds'.")]
        public void TryParse_HoursMinutesSeconds()
        {
            TestTryParse("2:4:30", new MetricLength(2, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Minutes:Seconds'.")]
        public void TryParse_MinutesSeconds()
        {
            TestTryParse("4:30", new MetricLength(0, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Milliseconds'.")]
        public void TryParse_Milliseconds()
        {
            TestTryParse("430", new MetricLength(0, 0, 0, 430));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void Parse_ToString()
        {
            var expectedLength = new MetricLength(3, 6, 8, 987);
            Assert.AreEqual(expectedLength, MetricLength.Parse(expectedLength.ToString()));
        }

        #endregion

        #region Private methods

        private static void TestTryParse(string input, MetricLength expectedLength)
        {
            MetricLength.TryParse(input, out var actualTime);
            Assert.AreEqual(expectedLength, actualTime);
        }

        #endregion
    }
}
