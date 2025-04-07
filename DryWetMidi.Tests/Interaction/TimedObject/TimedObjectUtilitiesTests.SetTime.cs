using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

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

            var result = timedEvent.SetTime((MidiTimeSpan)time, Factory.TempoMap);

            ClassicAssert.AreSame(timedEvent, result, "Result is not the same object.");
            ClassicAssert.AreEqual(time, result.Time, "Invalid time.");
        }

        [Test]
        public void SetTime_Note_Midi([Values(0, 100)] long time)
        {
            var note = Factory.GetNote("100", "50");

            var result = note.SetTime((MidiTimeSpan)time, Factory.TempoMap);

            ClassicAssert.AreSame(note, result, "Result is not the same object.");
            ClassicAssert.AreEqual(time, result.Time, "Invalid time.");
            ClassicAssert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_Chord_Midi([Values(0, 100)] long time)
        {
            var chord = Factory.GetChord(
                "100", "50",
                "110", "40");

            var result = chord.SetTime((MidiTimeSpan)time, Factory.TempoMap);

            ClassicAssert.AreSame(chord, result, "Result is not the same object.");
            ClassicAssert.AreEqual(time, result.Time, "Invalid time.");
            ClassicAssert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_TimedEvent_Metric([Values(0, 250000)] int ms)
        {
            var timedEvent = Factory.GetTimedEvent("100");

            var result = timedEvent.SetTime(new MetricTimeSpan(0, 0, 0, ms), Factory.TempoMap);

            ClassicAssert.AreSame(timedEvent, result, "Result is not the same object.");
            ClassicAssert.AreEqual(
                new MetricTimeSpan(0, 0, 0, ms),
                result.TimeAs<MetricTimeSpan>(Factory.TempoMap),
                "Invalid time.");
        }

        [Test]
        public void SetTime_NullObject()
        {
            var timedEvent = default(TimedEvent);

            ClassicAssert.Throws<ArgumentNullException>(() => timedEvent.SetTime(new MidiTimeSpan(), Factory.TempoMap));
        }

        [Test]
        public void SetTime_NullTime()
        {
            var timedEvent = Factory.GetTimedEvent("10");

            ClassicAssert.Throws<ArgumentNullException>(() => timedEvent.SetTime(null, Factory.TempoMap));
        }

        [Test]
        public void SetTime_NullTempoMap()
        {
            var timedEvent = Factory.GetTimedEvent("10");

            ClassicAssert.Throws<ArgumentNullException>(() => timedEvent.SetTime(new MidiTimeSpan(), null));
        }

        #endregion
    }
}
