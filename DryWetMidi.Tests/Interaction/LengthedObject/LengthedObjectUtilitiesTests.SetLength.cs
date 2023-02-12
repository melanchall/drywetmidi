using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class LengthedObjectUtilitiesTests
    {
        #region Constants

        private static readonly ObjectsFactory Factory = ObjectsFactory.Default;

        #endregion

        #region Test methods

        [Test]
        public void SetLength_Note_Midi(
            [Values(0, 100)] long initialTime,
            [Values(0, 100)] long initialLength,
            [Values(0, 1000)] long newLength)
        {
            var note = Factory.GetNote(initialTime.ToString(), initialLength.ToString());
            var result = note.SetLength((MidiTimeSpan)newLength, Factory.TempoMap);

            Assert.AreSame(note, result, "Result is not the same object.");
            Assert.AreEqual(newLength, result.Length, "Invalid length.");
            Assert.AreEqual(initialTime, result.Time, "Invalid time.");
        }

        [Test]
        public void SetLength_Chord_Midi(
            [Values(0, 100)] long initialTime,
            [Values(0, 100)] long initialLength,
            [Values(0, 1000)] long newLength)
        {
            var chord = Factory.GetChord(
                initialTime.ToString(), initialLength.ToString(),
                initialTime.ToString(), initialLength.ToString());
            var result = chord.SetLength((MidiTimeSpan)newLength, Factory.TempoMap);

            Assert.AreSame(chord, result, "Result is not the same object.");
            Assert.AreEqual(newLength, result.Length, "Invalid length.");
            Assert.AreEqual(initialTime, result.Time, "Invalid time.");
        }

        [Test]
        public void SetLength_Note_Metric(
            [Values(0, 250000)] int initialTimeMs,
            [Values(0, 500000)] int initialLengthMs,
            [Values(0, 1000000)] int newLengthMs)
        {
            var note = Factory.GetNote($"0:0:0:{initialTimeMs}", $"0:0:0:{initialLengthMs}");
            var result = note.SetLength(new MetricTimeSpan(0, 0, 0, newLengthMs), Factory.TempoMap);

            Assert.AreSame(note, result, "Result is not the same object.");
            Assert.AreEqual(
                new MetricTimeSpan(0, 0, 0, newLengthMs),
                result.LengthAs<MetricTimeSpan>(Factory.TempoMap),
                "Invalid length.");
            Assert.AreEqual(
                new MetricTimeSpan(0, 0, 0, initialTimeMs),
                result.TimeAs<MetricTimeSpan>(Factory.TempoMap),
                "Invalid time.");
        }

        [Test]
        public void SetLength_NullObject()
        {
            var note = default(Note);
            Assert.Throws<ArgumentNullException>(() => note.SetLength(new MidiTimeSpan(), Factory.TempoMap));
        }

        [Test]
        public void SetLength_NullLength()
        {
            var note = Factory.GetNote("10", "10");
            Assert.Throws<ArgumentNullException>(() => note.SetLength(null, Factory.TempoMap));
        }

        [Test]
        public void SetLength_NullTempoMap()
        {
            var note = Factory.GetNote("10", "10");
            Assert.Throws<ArgumentNullException>(() => note.SetLength(new MidiTimeSpan(), null));
        }

        #endregion
    }
}
