using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
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

        #region GetTimedEvents

        [Test]
        public void GetTimedEvents_MidiEventsCollection_Materialized_Empty()
        {
            var timedEvents = Enumerable.Empty<MidiEvent>().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_MidiEventsCollection_Materialized_OneEvent(long deltaTime)
        {
            var midiEvents = new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            };

            var timedEvents = midiEvents.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_MidiEventsCollection_Materialized_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var midiEvents = new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            };

            var timedEvents = midiEvents.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_MidiEventsCollection_NonMaterialized_Empty()
        {
            var timedEvents = GetNonMaterializedEmptyMidiEventsCollection().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_MidiEventsCollection_NonMaterialized_OneEvent(long deltaTime)
        {
            var midiEvents = GetNonMaterializedSingleMidiEventsCollection(deltaTime);
            var timedEvents = midiEvents.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_MidiEventsCollection_NonMaterialized_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var midiEvents = GetNonMaterializedMultipleMidiEventsCollection(deltaTime1, deltaTime2);
            var timedEvents = midiEvents.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunk_FromMaterialized_Empty()
        {
            var timedEvents = new TrackChunk(Enumerable.Empty<MidiEvent>()).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunk_FromMaterialized_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = trackChunk.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunk_FromMaterialized_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = trackChunk.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunk_FromNonMaterialized_Empty()
        {
            var timedEvents = new TrackChunk(GetNonMaterializedEmptyMidiEventsCollection()).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunk_FromNonMaterialized_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(GetNonMaterializedSingleMidiEventsCollection(deltaTime));
            var timedEvents = trackChunk.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunk_FromNonMaterialized_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(GetNonMaterializedMultipleMidiEventsCollection(deltaTime1, deltaTime2));
            var timedEvents = trackChunk.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        #endregion

        #endregion

        #region Private methods

        private IEnumerable<MidiEvent> GetNonMaterializedEmptyMidiEventsCollection()
        {
            yield break;
        }

        private IEnumerable<MidiEvent> GetNonMaterializedSingleMidiEventsCollection(long deltaTime)
        {
            yield return new NoteOnEvent { DeltaTime = deltaTime };
        }

        private IEnumerable<MidiEvent> GetNonMaterializedMultipleMidiEventsCollection(long deltaTime1, long deltaTime2)
        {
            yield return new NoteOnEvent { DeltaTime = deltaTime1 };
            yield return new NoteOffEvent { DeltaTime = deltaTime2 };
        }

        #endregion
    }
}
