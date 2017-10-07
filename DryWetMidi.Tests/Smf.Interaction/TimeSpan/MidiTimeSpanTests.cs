using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiTimeSpanTests
    {
        #region Test methods

        [TestMethod]
        [Description("Subtract MIDI time span.")]
        public void Subtract_Midi()
        {
            var actual = ((MidiTimeSpan)300).Subtract((MidiTimeSpan)40, TimeSpanMode.TimeLength);
            var expected = (MidiTimeSpan)260;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI time span.")]
        public void Subtract_Musical()
        {
            var actual = ((MidiTimeSpan)300).Subtract(MusicalTimeSpan.Quarter.SingleDotted(), TimeSpanMode.LengthLength);
            var expected = new MathTimeSpan((MidiTimeSpan)300,
                                            MusicalTimeSpan.Quarter.SingleDotted(),
                                            MathOperation.Subtract,
                                            TimeSpanMode.LengthLength);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Calculate difference between MIDI and metric time spans.")]
        public void Subtract_Metric_TimeTime_TempoChanged()
        {
            // Check that t2 + (t1 - t2) == t1 in MIDI ticks

            var time1 = new MidiTimeSpan(30000);
            var time2 = new MetricTimeSpan(0, 1, 0);

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 0, 30), Tempo.FromBeatsPerMinute(200));
                tempoMapManager.SetTempo(new MetricTime(0, 2, 0), Tempo.FromBeatsPerMinute(90));

                var tempoMap = tempoMapManager.TempoMap;

                var expected = TimeConverter2.ConvertFrom(time1, tempoMap);

                var length = time1.Subtract(time2, TimeSpanMode.TimeTime);
                var actual = TimeConverter2.ConvertFrom(time2.Add(length, TimeSpanMode.TimeLength), tempoMap);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [Description("Multiply MIDI time span.")]
        public void Multiply()
        {
            var actual = ((MidiTimeSpan)350).Multiply(3);
            var expected = (MidiTimeSpan)1050;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Multiply MIDI time span by double value.")]
        public void Multiply_Double()
        {
            var actual = ((MidiTimeSpan)350).Multiply(3.5);
            var expected = (MidiTimeSpan)1225;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Divide MIDI time span.")]
        public void Divide()
        {
            var actual = ((MidiTimeSpan)450).Divide(3);
            var expected = (MidiTimeSpan)150;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Divide MIDI time span by double value.")]
        public void Divide_Double()
        {
            var actual = ((MidiTimeSpan)1225).Divide(3.5);
            var expected = (MidiTimeSpan)350;

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
