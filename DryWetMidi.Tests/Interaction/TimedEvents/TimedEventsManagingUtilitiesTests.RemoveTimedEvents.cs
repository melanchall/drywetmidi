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
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[0],
                match: e => predicateValue,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_OneEvent_Matched([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOn,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOff,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_1([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[] { new NoteOffEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_2([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_3([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 }, new NoteOffEvent { DeltaTime = 150 }, new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOnEvent { DeltaTime = 350, NoteNumber = (SevenBitNumber)23 } },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_Big_RemoveHalf([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: Enumerable.Range(0, 5000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 5000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray(),
                expectedRemovedCount: 5000);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithPredicate_Big_RemoveAll([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithPredicate(
                containerType,
                midiEvents: Enumerable.Range(0, 10000).Select(i => new NoteOnEvent()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 10000);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[0]);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutPredicate_OneEvent([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() });
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutPredicate_Big_RemoveAll([Values] ContainerType containerType)
        {
            RemoveTimedEvents_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: Enumerable.Range(0, 10000).Select(i => new NoteOnEvent()).ToArray());
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                match: e => true,
                expectedMidiEvents: new MidiEvent[0][],
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_OneEvent_1_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_OneEvent_1_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => false,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_OneEvent_2_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_OneEvent_2_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => false,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_SomeMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[0]
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_SomeMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_SomeMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_AllMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_AllMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_MultipleEvents_AllMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Time >= 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 7);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_WithPredicate_Big_RemoveHalf([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 5000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 5000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray()).ToArray(),
                expectedRemovedCount: 50000);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithPredicate_Big_RemoveAll([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 10000).Select(i => new NoteOnEvent()).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(i => new MidiEvent[0]).ToArray(),
                expectedRemovedCount: 100000);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_EmptyTrackChunks([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_OneEvent_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_OneEvent_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_MultipleEvents_AllMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_MultipleEvents_AllMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_MultipleEvents_AllMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutPredicate_Big_RemoveAll([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 10000).Select(i => new NoteOnEvent()).ToArray()).ToArray());
        }

        #endregion

        #region Private methods

        private void RemoveTimedEvents_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedRemovedCount,
                            eventsCollection.RemoveTimedEvents(match),
                            "Invalid count of removed timed events.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedRemovedCount,
                            trackChunk.RemoveTimedEvents(match),
                            "Invalid count of removed timed events.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        RemoveTimedEvents_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            match,
                            new[] { expectedMidiEvents },
                            expectedRemovedCount);
                    }
                    break;
            }
        }

        private void RemoveTimedEvents_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            midiEvents.Count,
                            eventsCollection.RemoveTimedEvents(),
                            "Invalid count of removed timed events.");

                        var expectedEventsCollection = new EventsCollection();
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            midiEvents.Count,
                            trackChunk.RemoveTimedEvents(),
                            "Invalid count of removed timed events.");

                        var expectedTrackChunk = new TrackChunk();
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        RemoveTimedEvents_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents });
                    }
                    break;
            }
        }

        private void RemoveTimedEvents_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void RemoveTimedEvents_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(new MidiFile(midiEvents.Select(e => new TrackChunk())), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(midiEvents.Select(e => new TrackChunk()), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
