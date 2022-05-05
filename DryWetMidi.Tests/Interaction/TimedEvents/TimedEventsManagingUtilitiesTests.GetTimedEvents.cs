using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    // TODO: get rid of duplicated MidiAsserts.AreEqual
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
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_EventsCollection_OneEvent_Custom([Values(0, 100, 100000)] long deltaTime)
        {
            var eventsCollection = new EventsCollection
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            };

            var timedEvents = eventsCollection.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime, 0)
                },
                timedEvents,
                false,
                0,
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
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_EventsCollection_MultipleEvents_Custom_1([Values(0, 100)] long deltaTime1, [Values(0, 100)] long deltaTime2)
        {
            var eventsCollection = new EventsCollection
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            };

            var timedEvents = eventsCollection.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime1, 0),
                    new CustomTimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2, 0)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_EventsCollection_MultipleEvents_Custom_2([Values(0, 100)] long deltaTime1, [Values(0, 100)] long deltaTime2)
        {
            var eventsCollection = new EventsCollection
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            };

            var timedEvents = eventsCollection.GetTimedEvents(CustomEventSettings2);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent2(new NoteOnEvent(), deltaTime1, 0, 0),
                    new CustomTimedEvent2(new NoteOffEvent(), deltaTime1 + deltaTime2, 0, 1)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunk_Empty()
        {
            var timedEvents = new TrackChunk(Enumerable.Empty<MidiEvent>()).GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunk_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = trackChunk.GetTimedEvents();
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunk_OneEvent_Custom([Values(0, 100, 100000)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = trackChunk.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime, 0)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunk_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = trackChunk.GetTimedEvents();
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunk_MultipleEvents_Custom([Values(0, 100)] long deltaTime1, [Values(0, 100)] long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = trackChunk.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime1, 0),
                    new CustomTimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2, 0)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_Empty()
        {
            var timedEvents = Enumerable.Empty<TrackChunk>().GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_Empty()
        {
            var timedEvents = new[] { new TrackChunk() }.GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_OneEvent(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents();
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_OneEvent_Custom([Values(0, 100, 100000)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime, 0)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_MultipleEvents(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents();
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_MultipleEvents_Custom(
            [Values(0, 100)] long deltaTime1,
            [Values(0, 100)] long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime1, 0),
                    new CustomTimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2, 0)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_OneTrackChunk_MultipleEvents_Custom_Fail(
            [Values(0, 100)] long deltaTime1,
            [Values(0, 100)] long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk }.GetTimedEvents(CustomEventSettings);
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), deltaTime1),
                    new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2)
                },
                timedEvents,
                false,
                0,
                "Timed events are invalid."));
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_Empty_Empty()
        {
            var timedEvents = new[] { new TrackChunk(), new TrackChunk() }.GetTimedEvents();
            CollectionAssert.IsEmpty(timedEvents, "Timed events collection is not empty.");
        }

        [TestCase(0)]
        [TestCase(100)]
        [TestCase(100000)]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_Empty(long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new[] { trackChunk, new TrackChunk() }.GetTimedEvents();
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 100)]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_Empty(long deltaTime1, long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new[] { trackChunk, new TrackChunk() }.GetTimedEvents();
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_1() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 0),
                    new TimedEvent(new ProgramChangeEvent(), 0),
                });

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_1_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_2()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_2_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                100,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_3()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_3_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_4()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_4_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                100,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_5()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_5_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_6()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_6_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                100,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_7()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_7_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                100,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 200, 0),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_8()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_9()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_10()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent_11()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_1_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOnEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_5_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 1),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 0),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_6_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                100,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 1),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_1()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_1_Custom_1() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new ControlChangeEvent(), 0, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_1_Custom_2() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent2(new NoteOnEvent(), 0, 0, 0),
                    new CustomTimedEvent2(new NoteOffEvent(), 0, 0, 1),
                    new CustomTimedEvent2(new ProgramChangeEvent(), 0, 1, 0),
                    new CustomTimedEvent2(new ControlChangeEvent(), 0, 1, 1),
                },
                CustomEventSettings2);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_2()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_3()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_4()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_5()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_6()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_6_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                100,
                0,
                100,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                    new CustomTimedEvent(new ControlChangeEvent(), 100, 1),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_7()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_8()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_9()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_9_Custom() =>
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new ControlChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

        [Test]
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_10()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_11()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_12()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_13()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_14()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_15()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_16()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_17()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_18()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_19()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
        public void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents_20()
        {
            GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
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
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_MidiFile_OneTrackChunk_OneEvent_Custom([Values(0, 100, 100000)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime }
            });

            var timedEvents = new MidiFile(trackChunk).GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime, 0)
                },
                timedEvents,
                false,
                0,
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
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
                "Timed events are invalid.");
        }

        [Test]
        public void GetTimedEvents_MidiFile_OneTrackChunk_MultipleEvents_Custom(
            [Values(0, 100)] long deltaTime1,
            [Values(0, 100)] long deltaTime2)
        {
            var trackChunk = new TrackChunk(new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = deltaTime1 },
                new NoteOffEvent { DeltaTime = deltaTime2 },
            });

            var timedEvents = new MidiFile(trackChunk).GetTimedEvents(CustomEventSettings);
            MidiAsserts.AreEqual(
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), deltaTime1, 0),
                    new CustomTimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2, 0)
                },
                timedEvents,
                false,
                0,
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
            MidiAsserts.AreEqual(new[] { new TimedEvent(new NoteOnEvent(), deltaTime) }, timedEvents, false, 0, "Timed events are invalid.");
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
            MidiAsserts.AreEqual(
                new[] { new TimedEvent(new NoteOnEvent(), deltaTime1), new TimedEvent(new NoteOffEvent(), deltaTime1 + deltaTime2) },
                timedEvents,
                false,
                0,
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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_1_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_3_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                0,
                100,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent_5_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
                100,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_1_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOnEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 1),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents_5_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
                100,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 1),
                    new CustomTimedEvent(new ProgramChangeEvent(), 100, 0),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_1_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                0,
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new NoteOnEvent(), 0, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 0, 0),
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new ControlChangeEvent(), 0, 1),
                },
                CustomEventSettings);

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
        public void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents_9_Custom() =>
            GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
                100,
                0,
                0,
                0,
                new[]
                {
                    new CustomTimedEvent(new ProgramChangeEvent(), 0, 1),
                    new CustomTimedEvent(new ControlChangeEvent(), 0, 1),
                    new CustomTimedEvent(new NoteOnEvent(), 100, 0),
                    new CustomTimedEvent(new NoteOffEvent(), 100, 0),
                },
                CustomEventSettings);

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

        private void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_MultipleEvents(
            long aDeltaTime1,
            long aDeltaTime2,
            long bDeltaTime1,
            long bDeltaTime2,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_MultipleEvents(
            long aDeltaTime1,
            long aDeltaTime2,
            long bDeltaTime1,
            long bDeltaTime2,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        private void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_MultipleEvents_OneEvent(
            long aDeltaTime1,
            long aDeltaTime2,
            long bDeltaTime,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_MultipleEvents_OneEvent(
            long aDeltaTime1,
            long aDeltaTime2,
            long bDeltaTime,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        private void GetTimedEvents_TrackChunksCollection_MultipleTrackChunks_OneEvent_MultipleEvents(
            long aDeltaTime,
            long bDeltaTime1,
            long bDeltaTime2,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new[] { aTrackChunk, bTrackChunk }.GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        private void GetTimedEvents_MidiFile_MultipleTrackChunks_OneEvent_MultipleEvents(
            long aDeltaTime,
            long bDeltaTime1,
            long bDeltaTime2,
            ICollection<TimedEvent> expectedTimedEvents,
            TimedEventDetectionSettings settings = null)
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

            var timedEvents = new MidiFile(aTrackChunk, bTrackChunk).GetTimedEvents(settings);
            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events are invalid.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedEvents.Any(te => object.ReferenceEquals(te.Event, e))),
                "There are original events references.");

            var timedObjects = new[] { aTrackChunk, bTrackChunk }.GetObjects(ObjectType.TimedEvent, new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = settings
            });
            MidiAsserts.AreEqual(expectedTimedEvents, timedObjects, false, 0, "Timed events are invalid from GetObjects.");
            Assert.IsFalse(
                aTrackChunk.Events.Concat(bTrackChunk.Events).Any(e => timedObjects.Any(te => object.ReferenceEquals(((TimedEvent)te).Event, e))),
                "There are original events references from GetObjects.");
        }

        #endregion
    }
}
