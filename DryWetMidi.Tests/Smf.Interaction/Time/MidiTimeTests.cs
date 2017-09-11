using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiTimeTests
    {
        #region TestMethods

        [TestMethod]
        [Description("Try parse MIDI time in form of 'time'.")]
        public void TryParse_Valid()
        {
            TimeParsingTester.TestTryParse("123", (MidiTime)123);
        }

        [TestMethod]
        [Description("Try parse negative MIDI time.")]
        public void TryParse_Inalid_Negative()
        {
            Assert.IsFalse(MidiTime.TryParse("-234", out var time));
        }

        [TestMethod]
        [Description("Try parse NaN time.")]
        public void TryParse_Inalid_NaN()
        {
            Assert.IsFalse(MidiTime.TryParse("abc", out var time));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            TimeParsingTester.TestToString(new MidiTime(987));
        }

        [TestMethod]
        [Description("Calculate difference between two MIDI times.")]
        public void Subtract_Midi()
        {
            var time1 = new MidiTime(300);
            var time2 = new MidiTime(150);

            Assert.AreEqual(new MidiLength(150), time1.Subtract(time2, TempoMap.Default));
        }

        [TestMethod]
        [Description("Calculate difference between MIDI and metric times.")]
        public void Subtract_Metric_TempoChanged()
        {
            var time1 = new MidiTime(30000);
            var time2 = new MetricTime(0, 1, 0);

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
