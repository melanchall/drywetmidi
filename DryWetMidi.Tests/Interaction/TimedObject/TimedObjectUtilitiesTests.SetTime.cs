using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Constants

        private static readonly ObjectsFactory Factory = ObjectsFactory.Default;

        #endregion

        #region Test methods

        [Test]
        public void SetTime_TimedEvent_Midi([Values(0, 100)] long time)
        {
            var timedEvent = Factory.GetTimedEvent("100");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(timedEvent, (MidiTimeSpan)time, TempoMap.Default);

            Assert.AreSame(timedEvent, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
        }

        [Test]
        public void SetTime_Note_Midi([Values(0, 100)] long time)
        {
            var note = Factory.GetNote("100", "50");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(note, (MidiTimeSpan)time, TempoMap.Default);

            Assert.AreSame(note, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
            Assert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_Chord_Midi([Values(0, 100)] long time)
        {
            var chord = Factory.GetChord(
                "100", "50",
                "110", "40");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(chord, (MidiTimeSpan)time, TempoMap.Default);

            Assert.AreSame(chord, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
            Assert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_TimedEvent_Metric([Values(0, 250000)] int ms)
        {
            var timedEvent = Factory.GetTimedEvent("100");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(timedEvent, new MetricTimeSpan(0, 0, 0, ms), TempoMap.Default);

            Assert.AreSame(timedEvent, result, "Result is not the same object.");
            Assert.AreEqual(
                new MetricTimeSpan(0, 0, 0, ms),
                result.TimeAs<MetricTimeSpan>(TempoMap.Default),
                "Invalid time.");
        }

        #endregion
    }
}
