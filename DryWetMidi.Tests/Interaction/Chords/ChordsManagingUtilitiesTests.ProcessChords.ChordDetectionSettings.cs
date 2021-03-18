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
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 },
            midiEvents: new MidiEvent[0],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_1([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_2([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_3([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 0 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance - 1 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_5([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Time = 100,
            match: c => c.Time == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = notesTolerance + 1 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 - (notesTolerance + 1) },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_6([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Time = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_1([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_2([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_3([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_4([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Time = 100,
            match: c => c.Time == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 90 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_5([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)8,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)8 },
                new TextEvent("A"),
                new NoteOffEvent { Channel = (FourBitNumber)8 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_6([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)8,
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
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_7([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => c.Channel = (FourBitNumber)8,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10, Channel = (FourBitNumber)8 },
                new TextEvent("A"),
                new NoteOffEvent { Channel = (FourBitNumber)8 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 },
            midiEvents: new MidiEvent[0],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_2([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 0 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => c.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_3([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
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
            action: c => c.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = notesTolerance - 1 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
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
            action: c => c.Time = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_1([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_2([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
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
            action: c => c.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_3([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
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
            action: c => c.Channel = (FourBitNumber)8,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)8 },
                new TextEvent("A"),
                new NoteOffEvent { Channel = (FourBitNumber)8 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_4([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
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
            action: c => c.Channel = (FourBitNumber)8,
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

        [Test]
        public void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_5([Values] ContainerType containerType) => ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => c.Channel = (FourBitNumber)8,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10, Channel = (FourBitNumber)8 },
                new TextEvent("A"),
                new NoteOffEvent { Channel = (FourBitNumber)8 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                new TextEvent("B"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
            });

        #endregion

        #region Private methods

        private void ProcessChords_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<Chord> action,
            Predicate<Chord> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessChords(action, match, settings),
                            "Invalid count of processed chords.");

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
                            expectedProcessedCount,
                            trackChunk.ProcessChords(action, match, settings),
                            "Invalid count of processed chords.");

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
                        ProcessChords_DetectionSettings_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount);
                    }
                    break;
            }
        }

        private void ProcessChords_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<Chord> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var chordsCount = midiEvents.GetChords(settings).Count;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        Assert.AreEqual(
                            chordsCount,
                            eventsCollection.ProcessChords(action, settings),
                            "Invalid count of processed chords.");

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
                        var trackChunk = new TrackChunk(midiEvents);

                        Assert.AreEqual(
                            chordsCount,
                            trackChunk.ProcessChords(action, settings),
                            "Invalid count of processed chords.");

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
                        ProcessChords_DetectionSettings_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void ProcessChords_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Chord> action,
            Predicate<Chord> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessChords(action, match, settings),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessChords(action, match, settings),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessChords_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Chord> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var chordsCount = trackChunks.GetChords(settings).Count;

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    chordsCount,
                    midiFile.ProcessChords(action, settings),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    chordsCount,
                    trackChunks.ProcessChords(action, settings),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
