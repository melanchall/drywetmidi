using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        #region SetTime

        [Test]
        public void SetTime_Zero()
        {
            var tempoMap = TempoMap.Default;
            var timedEvent = new TimedEvent(new NoteOnEvent(), 1000);
            var changedTimedEvent = timedEvent.SetTime(new MetricTimeSpan(), tempoMap);

            Assert.AreSame(timedEvent, changedTimedEvent, "Changed timed event is not the original one.");
            Assert.AreEqual(0, changedTimedEvent.Time, "Time is not zero.");
        }

        [Test]
        public void SetTime_NonZero()
        {
            var tempoMap = TempoMap.Default;
            var timedEvent = new TimedEvent(new NoteOnEvent(), 1000);
            var changedTimedEvent = timedEvent.SetTime(new MetricTimeSpan(0, 0, 2), tempoMap);

            Assert.AreSame(timedEvent, changedTimedEvent, "Changed timed event is not the original one.");
            Assert.AreEqual(changedTimedEvent.TimeAs<MetricTimeSpan>(tempoMap), new MetricTimeSpan(0, 0, 2), "Time is invalid.");
        }

        #endregion

        #endregion
    }
}
