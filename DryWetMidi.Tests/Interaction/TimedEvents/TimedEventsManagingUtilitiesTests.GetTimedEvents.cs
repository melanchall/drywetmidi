using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

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

        #region Private methods

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
