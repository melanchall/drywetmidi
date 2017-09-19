using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiTimeSpanTests
    {
        #region TestMethods

        [TestMethod]
        [Description("Try parse MIDI time span in form of 'length'.")]
        public void TryParse_Valid()
        {
            TimeSpanTestingUtilities.TryParse("123", (MidiTimeSpan)123);
        }

        [TestMethod]
        [Description("Try parse negative MIDI time span.")]
        public void TryParse_Inalid_Negative()
        {
            Assert.IsFalse(MidiTimeSpan.TryParse("-234", out var length));
        }

        [TestMethod]
        [Description("Try parse NaN time span.")]
        public void TryParse_Inalid_NaN()
        {
            Assert.IsFalse(MidiTimeSpan.TryParse("abc", out var length));
        }

        [TestMethod]
        [Description("Parse string representation of a time span.")]
        public void Parse_ToString()
        {
            TimeSpanTestingUtilities.ParseToString(new MidiTimeSpan(987));
        }

        [TestMethod]
        [Description("Add MIDI time span.")]
        public void Add_Midi()
        {
            var actual = ((MidiTimeSpan)300).Add((MidiTimeSpan)40, MathOperationMode.TimeLength);
            var expected = (MidiTimeSpan)340;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Add non-MIDI time to MIDI time.")]
        public void Add_NonMidi_TimeTime()
        {
            TimeSpanTestingUtilities.Add_TimeTime((MidiTimeSpan)300, new MetricTimeSpan(0, 1, 30));
        }

        [TestMethod]
        [Description("Add non-MIDI length to MIDI time.")]
        public void Add_NonMidi_TimeLength()
        {
            TimeSpanTestingUtilities.Add_TimeLength((MidiTimeSpan)300, new MetricTimeSpan(0, 1, 30));
        }

        [TestMethod]
        [Description("Add non-MIDI length to MIDI length.")]
        public void Add_NonMidi_LengthLength()
        {
            TimeSpanTestingUtilities.Add_LengthLength((MidiTimeSpan)300, new MetricTimeSpan(0, 1, 30));
        }

        [TestMethod]
        [Description("Subtract MIDI time span.")]
        public void Subtract_Midi()
        {
            var actual = ((MidiTimeSpan)300).Subtract((MidiTimeSpan)40, MathOperationMode.TimeLength);
            var expected = (MidiTimeSpan)260;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI time span.")]
        public void Subtract_Musical()
        {
            var actual = ((MidiTimeSpan)300).Subtract(MusicalTimeSpan.Quarter.SingleDotted(), MathOperationMode.LengthLength);
            var expected = new MathTimeSpan((MidiTimeSpan)300,
                                            MusicalTimeSpan.Quarter.SingleDotted(),
                                            MathOperation.Subtract,
                                            MathOperationMode.LengthLength);

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

                var length = time1.Subtract(time2, MathOperationMode.TimeTime);
                var actual = TimeConverter2.ConvertFrom(time2.Add(length, MathOperationMode.TimeLength), tempoMap);

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
