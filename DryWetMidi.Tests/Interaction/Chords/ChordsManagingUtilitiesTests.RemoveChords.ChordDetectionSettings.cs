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
    public sealed partial class ChordsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 },
            midiEvents: new MidiEvent[0],
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_1([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_2([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_3([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 0 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance - 1 },
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_5([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance + 1 },
                new NoteOffEvent(),
            },
            match: c => c.Time == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = notesTolerance + 1 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_6([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance + 1 },
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_1([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_2([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_3([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_4([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            match: c => c.Time == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_5([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_6([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 },
            midiEvents: new MidiEvent[0],
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_2([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 0 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_3([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance - 1 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance + 1 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_1([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_2([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_3([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_4([Values] ContainerType containerType) => RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        #endregion

        #region Private methods

        private void RemoveChords_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Predicate<Chord> match,
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
                            eventsCollection.RemoveChords(match, settings),
                            "Invalid count of removed chords.");

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
                            trackChunk.RemoveChords(match, settings),
                            "Invalid count of removed chords.");

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
                        RemoveChords_DetectionSettings_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            match,
                            new[] { expectedMidiEvents },
                            expectedRemovedCount);
                    }
                    break;
            }
        }

        private void RemoveChords_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var chordsCount = eventsCollection.GetChords(settings).Count;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            chordsCount,
                            eventsCollection.RemoveChords(settings),
                            "Invalid count of removed chords.");

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
                            chordsCount,
                            trackChunk.RemoveChords(settings),
                            "Invalid count of removed chords.");

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
                        RemoveChords_DetectionSettings_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void RemoveChords_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<Chord> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveChords(match, settings),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveChords(match, settings),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void RemoveChords_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var chordsCount = trackChunks.GetChords(settings).Count;

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    chordsCount,
                    midiFile.RemoveChords(settings),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    chordsCount,
                    trackChunks.RemoveChords(settings),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
