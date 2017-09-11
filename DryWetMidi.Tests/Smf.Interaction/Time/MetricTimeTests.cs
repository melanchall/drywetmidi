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

        [TestMethod]
        [Description("Calculate difference between two metric times.")]
        public void Subtract_Metric()
        {
            var time1 = new MetricTime(0, 3, 30);
            var time2 = new MetricTime(0, 1, 25);

            Assert.AreEqual(new MetricLength(0, 2, 5), time1.Subtract(time2, TempoMap.Default));
        }

        [TestMethod]
        [Description("Calculate difference between metric and musical times.")]
        public void Subtract_Musical_TempoChanged()
        {
            var time1 = new MetricTime(0, 3, 30);
            var time2 = new MusicalTime(2, 0);

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 0, 30), Tempo.FromBeatsPerMinute(200));
                tempoMapManager.SetTempo(new MetricTime(0, 2, 0), Tempo.FromBeatsPerMinute(90));

                var tempoMap = tempoMapManager.TempoMap;

                var expected = TimeConverter.ConvertFrom(time1, tempoMap);

                var length = time1.Subtract(time2, tempoMap);
                var actual = TimeConverter.ConvertFrom(time2.Add(length), tempoMap);

                Assert.AreEqual(expected, actual);
            }
        }

        #endregion
    }
}
