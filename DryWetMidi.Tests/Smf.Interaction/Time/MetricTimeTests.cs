using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            TimeParsingTester.TestTryParse("2:4:30:567", new MetricTime(2, 4, 30, 567));
        }

        [TestMethod]
        [Description("Try parse metric time in form of 'Hours:Minutes:Seconds'.")]
        public void TryParse_HoursMinutesSeconds()
        {
            TimeParsingTester.TestTryParse("2:4:30", new MetricTime(2, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric time in form of 'Minutes:Seconds'.")]
        public void TryParse_MinutesSeconds()
        {
            TimeParsingTester.TestTryParse("4:30", new MetricTime(0, 4, 30));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            TimeParsingTester.TestToString(new MetricTime(3, 6, 8, 987));
        }

        #endregion
    }
}
