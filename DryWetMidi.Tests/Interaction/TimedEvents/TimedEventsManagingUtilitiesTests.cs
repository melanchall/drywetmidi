using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
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
        public void GetTimedEvents_EventsCollection_Empty()
        {
            var timedEvents = new EventsCollection().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_EventsCollection_OneEvent(long deltaTime)
        {
            var eventsCollection = new EventsCollection
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            };

            var timedEvents = eventsCollection.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_EventsCollection_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var eventsCollection = new EventsCollection
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            };

            var timedEvents = eventsCollection.GetTimedEvents();
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

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_Empty()
        {
            var timedEvents = Enumerable.Empty<TrackChunk>().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_OneTrackChunk_Empty()
        {
            var timedEvents = new[] { new TrackChunk() }.GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_Materialized_OneTrackChunk_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_Materialized_OneTrackChunk_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_Empty_Empty()
        {
            var timedEvents = new[] { new TrackChunk(), new TrackChunk() }.GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_Empty(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new[] { trackChunk, new TrackChunk() }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_Empty(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk, new TrackChunk() }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_1()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_2()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_3()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_4()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_5()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_6()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_7()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_8()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_9()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_10()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                150,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent_11()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                250,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                150,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                250,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_12()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_13()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_14()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_15()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_16()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_17()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                50,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 50),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_18()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_19()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                50,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_20()
        {
            GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_Empty()
        {
            var timedEvents = GetNonMaterializedEmptyTrackChunksCollection().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_OneTrackChunk_Empty()
        {
            var timedEvents = GetNonMaterializedSingleEmptyTrackChunksCollection().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_OneTrackChunk_OneEvent(long deltaTime)
        {
            var timedEvents = GetNonMaterializedSingleMidiEventSingleTrackChunksCollection(deltaTime).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_OneTrackChunk_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var timedEvents = GetNonMaterializedMultipleMidiEventsSingleTrackChunksCollection(deltaTime1, deltaTime2).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_Empty_Empty()
        {
            var timedEvents = GetNonMaterializedMultipleEmptyTrackChunksCollection().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_Empty(long deltaTime)
        {
            var timedEvents = GetNonMaterializedMultipleTrackChunksCollection_FirstWithOneEvent(deltaTime).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_Empty(long deltaTime1, long deltaTime2)
        {
            var timedEvents = GetNonMaterializedMultipleTrackChunksCollection_FirstWithMultipleEvents(deltaTime1, deltaTime2).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_1()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_2()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_3()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_4()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_5()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_6()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_7()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_8()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_9()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_10()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                150,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent_11()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                250,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                150,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
                250,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_12()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_13()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_14()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_15()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_16()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_17()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                50,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 50),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_18()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_19()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                50,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents_20()
        {
            GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_Empty()
        {
            var timedEvents = new MidiFile(Enumerable.Empty<TrackChunk>()).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [Test]
        public void GetTimedEvents_MidiFile_OneTrackChunk_Empty()
        {
            var timedEvents = new MidiFile(new[] { new TrackChunk() }).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_MidiFile_OneTrackChunk_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new MidiFile(trackChunk).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_MidiFile_OneTrackChunk_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new MidiFile(trackChunk).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_Empty_Empty()
        {
            var timedEvents = new MidiFile(new TrackChunk(), new TrackChunk()).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_Empty(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new MidiFile(trackChunk, new TrackChunk()).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_Empty(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new MidiFile(trackChunk, new TrackChunk()).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) }, timedEvents, false),
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_1()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_2()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_3()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_4()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_5()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_6()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_7()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_8()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_9()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_10()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                150,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_11()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                250,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_1()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_2()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_3()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_4()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_5()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_6()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_7()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_8()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_9()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_10()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                150,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_11()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                250,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ProgramChangeEvent(), 250),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_1()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_2()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_3()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_4()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_5()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_6()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_7()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_8()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_9()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_10()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_11()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_12()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_13()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                0,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new ControlChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_14()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                0,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_15()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_16()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 200),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_17()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                50,
                100,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 50),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_18()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                50,
                100,
                100,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 150),
                    new TimedEvent(new ControlChangeEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_19()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                50,
                100,
                new[]
                {
                    new TimedEvent(new ProgramChangeEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        [Test]
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_20()
        {
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                100,
                100,
                50,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 100),
                    new TimedEvent(new ProgramChangeEvent(), 100),
                    new TimedEvent(new ControlChangeEvent(), 150),
                    new TimedEvent(new NoteOffEvent(), 200),
                });
        }

        #endregion

        #region ProcessTimedEvents

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_EmptyTrackChunk()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_OneEvent_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_OneEvent_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_OneEvent_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_MultipleEvents_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_MultipleEvents_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_MultipleEvents_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate_MultipleEvents_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_EmptyTrackChunk()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_Matched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_NotMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_Matched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_Matched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_NotMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_OneEvent_NotMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 900 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90 },
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 700,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_EmptyTrackChunk()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_OneEvent_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_OneEvent_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_OneEvent_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_MultipleEvents_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_MultipleEvents_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_MultipleEvents_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate_MultipleEvents_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_EmptyTrackChunk()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_Matched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_NotMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_Matched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_Matched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_NotMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_OneEvent_NotMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_AllMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_SomeMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_NotMatched_NoProcessing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 3,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1, 2 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 },
                },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 }
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_NotMatched_Processing()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate_MultipleEvents_NotMatched_Processing_Time()
        {
            ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        #endregion

        #endregion

        #region Private methods

        private void ProcessTimedEvents_TrackChunk_WithIndices_WithPredicate(
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ICollection<int> expectedMatchedTotalIndices,
            ICollection<int> expectedMatchedIndices)
        {
            var trackChunk = new TrackChunk(midiEvents);

            var matchedTotalIndices = new List<int>();
            var matchedIndices = new List<int>();

            var totalIndices = new List<int>();

            Assert.AreEqual(
                expectedProcessedCount,
                trackChunk.ProcessTimedEvents(
                    (timedEvent, iTotal, iMatched) =>
                    {
                        matchedTotalIndices.Add(iTotal);
                        matchedIndices.Add(iMatched);
                        action(timedEvent);
                    },
                    (timedEvent, iTotal) =>
                    {
                        totalIndices.Add(iTotal);
                        return match(timedEvent);
                    }),
                "Invalid count of processed timed events.");
            Assert.IsTrue(MidiChunk.Equals(new TrackChunk(expectedMidiEvents), trackChunk, out var message), message);

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), totalIndices, "Invalid total indices.");

            CollectionAssert.AreEqual(expectedMatchedTotalIndices, matchedTotalIndices, "Invalid matched total indices.");
            CollectionAssert.AreEqual(expectedMatchedIndices, matchedIndices, "Invalid matched indices.");
        }

        private void ProcessTimedEvents_TrackChunk_WithIndices_WithoutPredicate(
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var trackChunk = new TrackChunk(midiEvents);

            var totalIndices = new List<int>();
            var matchedIndices = new List<int>();

            Assert.AreEqual(
                midiEvents.Count,
                trackChunk.ProcessTimedEvents((timedEvent, iTotal, iMatched) =>
                {
                    totalIndices.Add(iTotal);
                    matchedIndices.Add(iMatched);
                    action(timedEvent);
                }),
                "Invalid count of processed timed events.");
            Assert.IsTrue(MidiChunk.Equals(new TrackChunk(expectedMidiEvents), trackChunk, out var message), message);

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), totalIndices, "Invalid total indices.");
            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), matchedIndices, "Invalid matched indices.");
        }

        private void ProcessTimedEvents_TrackChunk_WithoutIndices_WithPredicate(
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var trackChunk = new TrackChunk(midiEvents);
            Assert.AreEqual(
                expectedProcessedCount,
                trackChunk.ProcessTimedEvents(action, match),
                "Invalid count of processed timed events.");
            Assert.IsTrue(MidiChunk.Equals(new TrackChunk(expectedMidiEvents), trackChunk, out var message), message);
        }

        private void ProcessTimedEvents_TrackChunk_WithoutIndices_WithoutPredicate(
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var trackChunk = new TrackChunk(midiEvents);
            Assert.AreEqual(
                midiEvents.Count,
                trackChunk.ProcessTimedEvents(action),
                "Invalid count of processed timed events.");
            Assert.IsTrue(MidiChunk.Equals(new TrackChunk(expectedMidiEvents), trackChunk, out var message), message);
        }

        private void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = aDeltaTime1 },
                new NoteOffEvent { DeltaTime = aDeltaTime2 },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = bDeltaTime1 },
                new ControlChangeEvent { DeltaTime = bDeltaTime2 },
            });

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = aDeltaTime1 },
                new NoteOffEvent { DeltaTime = aDeltaTime2 },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = bDeltaTime1 },
                new ControlChangeEvent { DeltaTime = bDeltaTime2 },
            });

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_MultipleEvents(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            IEnumerable<TrackChunk> GetTrackChunks()
            {
                yield return new TrackChunk(new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = aDeltaTime1 },
                    new NoteOffEvent { DeltaTime = aDeltaTime2 },
                });
                yield return new TrackChunk(new MidiEvent[]
                {
                    new ProgramChangeEvent { DeltaTime = bDeltaTime1 },
                    new ControlChangeEvent { DeltaTime = bDeltaTime2 },
                });
            }

            var timedEvents = GetTrackChunks().GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_MultipleEvents_OneEvent(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = aDeltaTime1 },
                new NoteOffEvent { DeltaTime = aDeltaTime2 },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = bDeltaTime },
            });

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = aDeltaTime1 },
                new NoteOffEvent { DeltaTime = aDeltaTime2 },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = bDeltaTime },
            });

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_MultipleEvents_OneEvent(
            long aDeltaTime1, long aDeltaTime2, long bDeltaTime, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            IEnumerable<TrackChunk> GetTrackChunks()
            {
                yield return new TrackChunk(new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = aDeltaTime1 },
                    new NoteOffEvent { DeltaTime = aDeltaTime2 },
                });
                yield return new TrackChunk(new MidiEvent[]
                {
                    new ProgramChangeEvent { DeltaTime = bDeltaTime },
                });
            }

            var timedEvents = GetTrackChunks().GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_TrackChunksCollection_Materialized_MultipleTrackChunks_OneEvent_MultipleEvents(
            long aDeltaTime, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = aDeltaTime },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = bDeltaTime1 },
                new NoteOffEvent { DeltaTime = bDeltaTime2 },
            });

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
            long aDeltaTime, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            var aTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new ProgramChangeEvent { DeltaTime = aDeltaTime },
            });

            var bTrackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = bDeltaTime1 },
                new NoteOffEvent { DeltaTime = bDeltaTime2 },
            });

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

        private void GetTimedEvents_TrackChunksCollection_NonMaterialized_MultipleTrackChunks_OneEvent_MultipleEvents(
            long aDeltaTime, long bDeltaTime1, long bDeltaTime2, IEnumerable<TimedEvent> expectedTimedEvents)
        {
            IEnumerable<TrackChunk> GetTrackChunks()
            {
                yield return new TrackChunk(new MidiEvent[]
                {
                    new ProgramChangeEvent { DeltaTime = aDeltaTime },
                });
                yield return new TrackChunk(new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = bDeltaTime1 },
                    new NoteOffEvent { DeltaTime = bDeltaTime2 },
                });
            }

            var timedEvents = GetTrackChunks().GetTimedEvents();
            Assert.IsTrue(
                TimedEventEquality.AreEqual(expectedTimedEvents, timedEvents, false),
                "Timed events are invalid.");
        }

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

        private IEnumerable<TrackChunk> GetNonMaterializedEmptyTrackChunksCollection()
        {
            yield break;
        }

        private IEnumerable<TrackChunk> GetNonMaterializedSingleEmptyTrackChunksCollection()
        {
            yield return new TrackChunk();
        }

        private IEnumerable<TrackChunk> GetNonMaterializedSingleMidiEventSingleTrackChunksCollection(long deltaTime)
        {
            yield return new TrackChunk(new NoteOnEvent { DeltaTime = deltaTime });
        }

        private IEnumerable<TrackChunk> GetNonMaterializedMultipleMidiEventsSingleTrackChunksCollection(long deltaTime1, long deltaTime2)
        {
            yield return new TrackChunk(new NoteOnEvent { DeltaTime = deltaTime1 }, new NoteOffEvent { DeltaTime = deltaTime2 });
        }

        private IEnumerable<TrackChunk> GetNonMaterializedMultipleEmptyTrackChunksCollection()
        {
            yield return new TrackChunk();
            yield return new TrackChunk();
        }

        private IEnumerable<TrackChunk> GetNonMaterializedMultipleTrackChunksCollection_FirstWithOneEvent(long deltaTime)
        {
            yield return new TrackChunk(new NoteOnEvent { DeltaTime = deltaTime });
            yield return new TrackChunk();
        }

        private IEnumerable<TrackChunk> GetNonMaterializedMultipleTrackChunksCollection_FirstWithMultipleEvents(long deltaTime1, long deltaTime2)
        {
            yield return new TrackChunk(new NoteOnEvent { DeltaTime = deltaTime1 }, new NoteOffEvent { DeltaTime = deltaTime2 });
            yield return new TrackChunk();
        }

        #endregion
    }
}
