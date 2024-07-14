using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Tests.Utilities;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Test methods

        [Test]
        public void RemoveObjects_TimedEventsAndNotes_WithPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            match: obj => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveObjects_TimedEventsAndNotes_WithPredicate_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 10 },
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            match: obj => obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text == "A",
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveObjects_TimedEventsAndChords_WithPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            match: obj => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 5,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void RemoveObjects_TimedEventsAndChords_WithPredicate_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            match: obj => obj is Chord,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("C"),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveObjects_TimedEventsAndChords_WithPredicate_3([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            match: obj => obj is Chord,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void RemoveObjects_TimedEventsAndNotes_WithoutPredicate([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveObjects_TimedEventsAndChords_WithoutPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void RemoveObjects_TimedEventsAndChords_WithoutPredicate_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[0],
            match: obj => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_SingleEvent_Matched([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
            },
            match: obj => obj is TimedEvent,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 1);

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_SingleEvent_NotMatched([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
            },
            match: obj => obj is Note,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 5 },
                new TextEvent("B") { DeltaTime = 6 },
                new TextEvent("CA") { DeltaTime = 7 },
                new TextEvent("D") { DeltaTime = 8 },
                new TextEvent("EA") { DeltaTime = 9 },
                new TextEvent("F") { DeltaTime = 10 },
            },
            match: obj => obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text.Contains("A"),
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("B") { DeltaTime = 11 },
                new TextEvent("D") { DeltaTime = 15 },
                new TextEvent("F") { DeltaTime = 19 },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_2([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A") { DeltaTime = 5 },
                    new TextEvent("B") { DeltaTime = 6 },
                    new TextEvent("CA") { DeltaTime = 7 },
                    new TextEvent("D") { DeltaTime = 8 },
                    new TextEvent("EA") { DeltaTime = 9 },
                    new TextEvent("F") { DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A") { DeltaTime = 5 },
                    new TextEvent("B") { DeltaTime = 6 },
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 8 },
                    new NoteOnEvent(),
                    new TextEvent("EA") { DeltaTime = 9 },
                    new NoteOffEvent(),
                    new TextEvent("F") { DeltaTime = 10 },
                },
            },
            match: obj => obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text.Contains("A"),
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("B") { DeltaTime = 11 },
                    new TextEvent("D") { DeltaTime = 15 },
                    new TextEvent("F") { DeltaTime = 19 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 11 },
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 8 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 9 },
                    new TextEvent("F") { DeltaTime = 10 },
                },
            },
            expectedRemovedCount: 5);

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_Custom_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            },
            match: obj => obj is CustomTimedEvent timedEvent && timedEvent.EventsCollectionIndex == 0 && timedEvent.EventIndex == 1,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("C") { DeltaTime = 10 },
                new TextEvent("D") { DeltaTime = 20 },
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                }
            });

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_Custom_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            },
            match: obj => obj is CustomTimedEvent timedEvent && timedEvent.EventsCollectionIndex == 1 && timedEvent.EventIndex == 1,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            },
            expectedRemovedCount: 0,
            settings: new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                }
            });

        [Test]
        public void RemoveObjects_TimedEvents_WithPredicate_Custom_3([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            match: obj => obj is CustomTimedEvent timedEvent && timedEvent.EventsCollectionIndex == 1 && timedEvent.EventIndex == 1,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C") { DeltaTime = 10 },
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                }
            });

        [Test]
        public void RemoveObjects_TimedEvents_WithoutPredicate_Custom([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            expectedMidiEvents: new []
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                },
            },
            settings: new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                }
            });

        [Test]
        public void RemoveObjects_Notes_WithPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new TextEvent("D") { DeltaTime = 20 },
            },
            match: obj => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveObjects_Notes_WithPredicate_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: obj => obj.Time < 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveObjects_Notes_WithPredicate_3([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 5 },
                    new TextEvent("CBA"),
                    new NoteOnEvent((SevenBitNumber)90, SevenBitNumber.MaxValue) { DeltaTime = 2 },
                    new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue) { DeltaTime = 2 },
                    new TextEvent("ABC"),
                    new NoteOnEvent((SevenBitNumber)9, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                    new NoteOffEvent((SevenBitNumber)9, SevenBitNumber.MinValue) { DeltaTime = 5 },
                },
            },
            match: obj => obj.Time < 10,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("CBA") { DeltaTime = 5 },
                    new TextEvent("ABC") { DeltaTime = 4 },
                    new NoteOnEvent((SevenBitNumber)9, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                    new NoteOffEvent((SevenBitNumber)9, SevenBitNumber.MinValue) { DeltaTime = 5 },
                },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveObjects_Notes_WithPredicate_4([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 1 },
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 20 },
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: obj => ((Note)obj).Length == 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B") { DeltaTime = 11 },
                new TextEvent("C"),
                new NoteOffEvent() { DeltaTime = 20 },
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        [Test]
        public void RemoveObjects_Notes_WithPredicate_5([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 1 },
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 20 },
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: obj => ((Note)obj).Length == 11,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent() { DeltaTime = 1 },
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent() { DeltaTime = 20 },
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                }
            });

        [Test]
        public void RemoveObjects_Notes_WithPredicate_Custom([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)60),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)0),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 5 },
                    new TextEvent("CBA"),
                    new NoteOnEvent((SevenBitNumber)90, (SevenBitNumber)40) { DeltaTime = 2 },
                    new NoteOffEvent((SevenBitNumber)90, (SevenBitNumber)60) { DeltaTime = 2 },
                    new TextEvent("ABC"),
                    new NoteOnEvent((SevenBitNumber)9, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                    new NoteOffEvent((SevenBitNumber)9, SevenBitNumber.MinValue) { DeltaTime = 5 },
                },
            },
            match: obj => ((CustomNote)obj).Velocity > 50 && ((CustomTimedEvent)((Note)obj).GetTimedNoteOnEvent()).EventIndex >= 1,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 5 },
                    new TextEvent("CBA"),
                    new NoteOnEvent((SevenBitNumber)90, (SevenBitNumber)40) { DeltaTime = 2 },
                    new NoteOffEvent((SevenBitNumber)90, (SevenBitNumber)60) { DeltaTime = 2 },
                    new TextEvent("ABC"),
                },
            },
            expectedRemovedCount: 2,
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    Constructor = data => new CustomNote(data.TimedNoteOnEvent, data.TimedNoteOffEvent, ((CustomTimedEvent)data.TimedNoteOnEvent).EventsCollectionIndex),
                },
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                },
            });

        [Test]
        public void RemoveObjects_Notes_WithoutPredicate_1([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            });

        [Test]
        public void RemoveObjects_Notes_WithoutPredicate_2([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 20 },
            });

        [Test]
        public void RemoveObjects_Notes_WithoutPredicate_3([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 5 },
                    new TextEvent("CBA"),
                    new NoteOnEvent((SevenBitNumber)90, SevenBitNumber.MaxValue) { DeltaTime = 2 },
                    new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue) { DeltaTime = 2 },
                    new TextEvent("ABC"),
                    new NoteOnEvent((SevenBitNumber)9, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                    new NoteOffEvent((SevenBitNumber)9, SevenBitNumber.MinValue) { DeltaTime = 5 },
                },
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
                new MidiEvent[]
                {
                    new TextEvent("CBA") { DeltaTime = 5 },
                    new TextEvent("ABC"){ DeltaTime = 4 },
                },
            });

        [Test]
        public void RemoveObjects_Notes_WithoutPredicate_4([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 1 },
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 20 },
                new TextEvent("D") { DeltaTime = 20 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 11 },
                new TextEvent("C"),
                new TextEvent("D") { DeltaTime = 40 },
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        [Test]
        public void RemoveObjects_Chords_WithPredicate_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            match: obj => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedRemovedCount: 1,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                }
            });

        [Test]
        public void RemoveObjects_Chords_WithPredicate_2([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            match: obj => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            expectedRemovedCount: 4);

        [Test]
        public void RemoveObjects_Chords_WithPredicate_3([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            match: obj => ((Chord)obj).Length == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveObjects_Chords_WithoutPredicate_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                }
            });

        [Test]
        public void RemoveObjects_Chords_WithoutPredicate_2([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            });

        [Test]
        public void RemoveObjects_Chords_WithoutPredicate_3([Values] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void RemoveObjects_Chords_WithPredicate_Custom([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => RemoveObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 34 },
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            match: obj => ((CustomChord)obj).EventsCollectionIndex == 1,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new TextEvent("D") { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)20, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)20, SevenBitNumber.MinValue),
                    new TextEvent("E") { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 44 },
                    new TextEvent("C"),
                    new TextEvent("D") { DeltaTime = 20 },
                },
            },
            expectedRemovedCount: 2,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    Constructor = data => new CustomChord(data.Notes, ((CustomNote)data.Notes.First()).EventsCollectionIndex),
                },
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    Constructor = data => new CustomNote(data.TimedNoteOnEvent, data.TimedNoteOffEvent, ((CustomTimedEvent)data.TimedNoteOnEvent).EventsCollectionIndex),
                },
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                },
            });

        #endregion

        #region Private methods

        private void RemoveObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    RemoveObjects_EventsCollection(
                        objectType,
                        midiEvents,
                        expectedMidiEvents,
                        settings);
                    break;

                case ContainerType.TrackChunk:
                    RemoveObjects_TrackChunk(
                        objectType,
                        midiEvents,
                        expectedMidiEvents,
                        settings);
                    break;

                default:
                    RemoveObjects(
                        containerType,
                        objectType,
                        new[] { midiEvents },
                        new[] { expectedMidiEvents },
                        settings);
                    break;
            }
        }

        private void RemoveObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    RemoveObjects_EventsCollection(
                        objectType,
                        midiEvents,
                        match,
                        expectedMidiEvents,
                        expectedRemovedCount,
                        settings);
                    break;

                case ContainerType.TrackChunk:
                    RemoveObjects_TrackChunk(
                        objectType,
                        midiEvents,
                        match,
                        expectedMidiEvents,
                        expectedRemovedCount,
                        settings);
                    break;

                default:
                    RemoveObjects(
                        containerType,
                        objectType,
                        new[] { midiEvents },
                        match,
                        new[] { expectedMidiEvents },
                        expectedRemovedCount,
                        settings);
                    break;
            }
        }

        private void RemoveObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            CollectionAssert.Contains(new[] { ContainerType.TrackChunks, ContainerType.File }, containerType);

            switch (containerType)
            {
                case ContainerType.TrackChunks:
                    RemoveObjects_TrackChunks(
                        objectType,
                        midiEvents,
                        expectedMidiEvents,
                        settings);
                    break;

                case ContainerType.File:
                    RemoveObjects_File(
                        objectType,
                        midiEvents,
                        expectedMidiEvents,
                        settings);
                    break;
            }
        }

        private void RemoveObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            CollectionAssert.Contains(new[] { ContainerType.TrackChunks, ContainerType.File }, containerType);

            switch (containerType)
            {
                case ContainerType.TrackChunks:
                    RemoveObjects_TrackChunks(
                        objectType,
                        midiEvents,
                        match,
                        expectedMidiEvents,
                        expectedRemovedCount,
                        settings);
                    break;

                case ContainerType.File:
                    RemoveObjects_File(
                        objectType,
                        midiEvents,
                        match,
                        expectedMidiEvents,
                        expectedRemovedCount,
                        settings);
                    break;
            }
        }

        private void RemoveObjects_EventsCollection(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            Assert.AreEqual(
                expectedRemovedCount,
                eventsCollection.RemoveObjects(objectType, match, settings),
                "Invalid count of removed objects for events collection.");

            var expectedEventsCollection = new EventsCollection();
            expectedEventsCollection.AddRange(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events collection is invalid.");
            CheckNoNewEventsReferences(midiEvents, eventsCollection);
        }

        private void RemoveObjects_TrackChunk(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            var trackChunk = new TrackChunk(midiEvents);

            Assert.AreEqual(
                expectedRemovedCount,
                trackChunk.RemoveObjects(objectType, match, settings),
                "Invalid count of removed objects for track chunk.");

            var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Track chunk is invalid.");
            CheckNoNewEventsReferences(midiEvents, trackChunk.Events);
        }

        private void RemoveObjects_TrackChunks(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            Assert.AreEqual(
                expectedRemovedCount,
                trackChunks.RemoveObjects(objectType, match, settings),
                "Invalid count of removed objects for track chunks.");

            MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Track chunks are invalid.");
            CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), trackChunks.SelectMany(c => c.Events));
        }

        private void RemoveObjects_File(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount,
            ObjectDetectionSettings settings = null)
        {
            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e)));

            Assert.AreEqual(
                expectedRemovedCount,
                midiFile.RemoveObjects(objectType, match, settings),
                "Invalid count of removed objects for file.");

            MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "File is invalid.");
            CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), midiFile.GetTrackChunks().SelectMany(c => c.Events));
        }

        private void RemoveObjects_EventsCollection(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var expectedRemovedCount = eventsCollection.GetObjects(objectType, settings).Count;

            Assert.AreEqual(
                expectedRemovedCount,
                eventsCollection.RemoveObjects(objectType, settings),
                "Invalid count of removed objects for events collection.");

            var expectedEventsCollection = new EventsCollection();
            expectedEventsCollection.AddRange(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events collection is invalid.");
            CheckNoNewEventsReferences(midiEvents, eventsCollection);
        }

        private void RemoveObjects_TrackChunk(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            var trackChunk = new TrackChunk(midiEvents);
            var expectedRemovedCount = trackChunk.GetObjects(objectType, settings).Count;

            Assert.AreEqual(
                expectedRemovedCount,
                trackChunk.RemoveObjects(objectType, settings),
                "Invalid count of removed objects for track chunk.");

            var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Track chunk is invalid.");
            CheckNoNewEventsReferences(midiEvents, trackChunk.Events);
        }

        private void RemoveObjects_TrackChunks(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var expectedRemovedCount = trackChunks.GetObjects(objectType, settings).Count;

            Assert.AreEqual(
                expectedRemovedCount,
                trackChunks.RemoveObjects(objectType, settings),
                "Invalid count of removed objects for track chunks.");

            MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Track chunks are invalid.");
            CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), trackChunks.SelectMany(c => c.Events));
        }

        private void RemoveObjects_File(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings = null)
        {
            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e)));
            var expectedRemovedCount = midiFile.GetObjects(objectType, settings).Count;

            Assert.AreEqual(
                expectedRemovedCount,
                midiFile.RemoveObjects(objectType, settings),
                "Invalid count of removed objects for file.");

            MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "File is invalid.");
            CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), midiFile.GetTrackChunks().SelectMany(c => c.Events));
        }

        private static void CheckNoNewEventsReferences(
            IEnumerable<MidiEvent> originalEvents,
            IEnumerable<MidiEvent> actualEvents) => Assert.IsTrue(
            actualEvents.All(e => originalEvents.Any(ee => object.ReferenceEquals(e, ee))),
            "There are new events references.");

        #endregion
    }
}
