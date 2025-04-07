using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessObjects_TimedEventsAndNotes_WithPredicate_1([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj =>
            {
                if (obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text == "A")
                    textEvent.Text = "B";
                else
                    obj.Time += 10;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessObjects_TimedEventsAndNotes_WithPredicate_2([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            match: obj => obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text == "A",
            action: obj => obj.Time += 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new TextEvent("A") { DeltaTime = 10 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_1([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj => obj.Time += obj is TimedEvent ? 10 : 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 10 },
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 5,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_2([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj => obj.Time += obj is TimedEvent ? 10 : 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 4);

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_3([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj => obj.Time += 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("C"),
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_TimedEventsAndNotes_WithoutPredicate([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            action: obj =>
            {
                if (obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text == "A")
                    textEvent.Text = "B";
                else
                    obj.Time += 10;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new TextEvent("C"),
            });

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithoutPredicate_1([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj => obj.Time += obj is TimedEvent ? 10 : 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 10 },
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithoutPredicate_2([Values] ContainerType containerType) => ProcessObjects(
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
            action: obj => obj.Time += obj is TimedEvent ? 10 : 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 10 },
                new TextEvent("C"),
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            });

        [Test]
        public void ProcessObjects_EmptyCollection_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[0],
            action: obj => obj.Time += 10,
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void ProcessObjects_EmptyCollection_2([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new MidiEvent[0],
            match: obj => obj.Time > 10,
            action: obj => obj.Time += 10,
            expectedMidiEvents: new MidiEvent[0],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_EmptyCollection_3([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            match: obj => obj.Time > 10,
            action: obj => obj.Time += 10,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_EmptyCollection_4([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            action: obj => obj.Time += 10,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            });

        [Test]
        public void ProcessObjects_Notes_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent(),
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            match: obj => true,
            action: obj => ((Note)obj).Velocity += (SevenBitNumber)10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent() { Velocity = (SevenBitNumber)10 },
                new NoteOnEvent() { Velocity = (SevenBitNumber)10 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Notes_2([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            match: obj => ((Note)obj).Length == 3,
            action: obj => ((Note)obj).Channel += (FourBitNumber)1,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent() { Channel = (FourBitNumber)1 },
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent() { Channel = (FourBitNumber)1 },
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Notes_3([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            match: obj => ((Note)obj).Length == 0,
            action: obj => ((Note)obj).Channel += (FourBitNumber)1,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3, Channel = (FourBitNumber)1 },
                new TextEvent("B"),
                new NoteOffEvent() { Channel = (FourBitNumber)1 },
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            expectedProcessedCount: 1,
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        [Test]
        public void ProcessObjects_Notes_Hint_None_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            match: obj => true,
            action: obj => ((Note)obj).Channel += (FourBitNumber)1,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { Channel = (FourBitNumber)1 },
                    new NoteOnEvent() { DeltaTime = 3, Channel = (FourBitNumber)1 },
                    new TextEvent("B"),
                    new NoteOffEvent() { Channel = (FourBitNumber)1 },
                    new NoteOffEvent() { DeltaTime = 5, Channel = (FourBitNumber)1 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10, Channel = (FourBitNumber)1 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5, Channel = (FourBitNumber)1 },
                },
            },
            expectedProcessedCount: 3,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Notes_Hint_None_2([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Notes_Hint_None_3([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Notes_Hint_Default_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 3 },
                    new NoteOffEvent(),
                    new TextEvent("C") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 5 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOnEvent() { DeltaTime = 20 },
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.Default);

        [Test]
        public void ProcessObjects_Notes_Hint_Default_2([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B") { DeltaTime = 3 },
                new NoteOffEvent(),
                new TextEvent("C") { DeltaTime = 5 },
                new NoteOnEvent() { DeltaTime = 15 },
                new NoteOffEvent() { DeltaTime = 5 },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.Default);

        [Test]
        public void ProcessObjects_Notes_Hint_Default_3([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOnEvent() { DeltaTime = 3 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
                new TextEvent("C"),
            },
            match: obj => true,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 3 },
                new TextEvent("C") { DeltaTime = 5 },
                new NoteOnEvent() { DeltaTime = 12 },
                new NoteOnEvent() { DeltaTime = 3 },
                new NoteOffEvent(),
                new NoteOffEvent() { DeltaTime = 5 },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.Default);

        [Test]
        public void ProcessObjects_Notes_Hint_TimeOrLengthCanBeChanged_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 3 },
                    new NoteOffEvent(),
                    new TextEvent("C") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 5 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                    new TextEvent("C"),
                    new NoteOnEvent() { DeltaTime = 20 },
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.TimeOrLengthCanBeChanged);

        [Test]
        public void ProcessObjects_Notes_Hint_NotesCollectionCanBeChanged_1([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Note,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            match: obj => obj.Time > 0,
            action: obj => obj.Time += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 3 },
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent() { DeltaTime = 5 },
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_Hint_TimeOrLengthCanBeChanged_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            match: obj => ((Chord)obj).Channel == 0,
            action: obj => ((Chord)obj).Length += 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent() { DeltaTime = 20 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.TimeOrLengthCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_Hint_NotesCollectionCanBeChanged_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            match: obj => ((Chord)obj).Channel == 0,
            action: obj => ((Chord)obj).Notes.Remove(((Chord)obj).Notes.First()),
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_Hint_NotesCollectionCanBeChanged_2([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            match: obj => ((Chord)obj).Channel == 0,
            action: obj => ((Chord)obj).Notes.First().Time += 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_Hint_NotesCollectionCanBeChanged_3([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            match: obj => ((Chord)obj).Channel == 0,
            action: obj => ((Chord)obj).Notes.Add(new Note((SevenBitNumber)70)),
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_Hint_None_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            match: obj => true,
            action: obj => ((Chord)obj).Notes.First().Time += 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_Hint_All([Values(ContainerType.TrackChunks, ContainerType.File)] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            match: obj => obj.Time == 0,
            action: obj =>
            {
                var chord = (Chord)obj;
                chord.Notes.Add(new Note((SevenBitNumber)90));
                chord.Time += 20;
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity),
                    new NoteOnEvent { DeltaTime = 30 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent() { DeltaTime = 20 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity),
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged | ObjectProcessingHint.TimeOrLengthCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_Custom([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent() { DeltaTime = 2 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 7 },
            },
            match: obj => ((CustomTimedEvent)((CustomNote)((CustomChord)obj).Notes.First()).GetTimedNoteOnEvent()).EventsCollectionIndex == 0,
            action: obj => obj.Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent() { DeltaTime = 2 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 7 },
            },
            expectedProcessedCount: 1,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = data => new CustomChord(
                        data.Notes,
                        (data.Notes.FirstOrDefault() as CustomNote)?.EventsCollectionIndex),
                },
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    Constructor = data => new CustomNote(
                        data.TimedNoteOnEvent,
                        data.TimedNoteOffEvent,
                        (data.TimedNoteOnEvent as CustomTimedEvent)?.EventsCollectionIndex),
                },
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
                }
            });

        [Test]
        public void ProcessObjects_Chords_NotesTolerance_1([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
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
            match: obj => obj.Time == 0,
            action: obj => obj.Time = 100,
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
            expectedProcessedCount: 1,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            });

        [Test]
        public void ProcessObjects_Chords_NotesMinCount_1([Values] ContainerType containerType) => ProcessObjects(
            containerType: containerType,
            objectType: ObjectType.Chord,
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
            match: obj => true,
            action: obj => ((Chord)obj).Channel = (FourBitNumber)8,
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
            expectedProcessedCount: 1,
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2 }
            });

        #endregion

        #region Private methods

        private void ProcessObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    ProcessObjects_EventsCollection(
                        objectType,
                        midiEvents,
                        action,
                        expectedMidiEvents,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                case ContainerType.TrackChunk:
                    ProcessObjects_TrackChunk(
                        objectType,
                        midiEvents,
                        action,
                        expectedMidiEvents,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                default:
                    ProcessObjects(
                        containerType,
                        objectType,
                        new[] { midiEvents },
                        action,
                        new[] { expectedMidiEvents },
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;
            }
        }

        private void ProcessObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    ProcessObjects_EventsCollection(
                        objectType,
                        midiEvents,
                        match,
                        action,
                        expectedMidiEvents,
                        expectedProcessedCount,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                case ContainerType.TrackChunk:
                    ProcessObjects_TrackChunk(
                        objectType,
                        midiEvents,
                        match,
                        action,
                        expectedMidiEvents,
                        expectedProcessedCount,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                default:
                    ProcessObjects(
                        containerType,
                        objectType,
                        new[] { midiEvents },
                        match,
                        action,
                        new[] { expectedMidiEvents },
                        expectedProcessedCount,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;
            }
        }

        private void ProcessObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false)
        {
            CollectionAssert.Contains(new[] { ContainerType.TrackChunks, ContainerType.File }, containerType);

            switch (containerType)
            {
                case ContainerType.TrackChunks:
                    ProcessObjects_TrackChunks(
                        objectType,
                        midiEvents,
                        action,
                        expectedMidiEvents,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                case ContainerType.File:
                    ProcessObjects_File(
                        objectType,
                        midiEvents,
                        action,
                        expectedMidiEvents,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;
            }
        }

        private void ProcessObjects(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false)
        {
            CollectionAssert.Contains(new[] { ContainerType.TrackChunks, ContainerType.File }, containerType);

            switch (containerType)
            {
                case ContainerType.TrackChunks:
                    ProcessObjects_TrackChunks(
                        objectType,
                        midiEvents,
                        match,
                        action,
                        expectedMidiEvents,
                        expectedProcessedCount,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;

                case ContainerType.File:
                    ProcessObjects_File(
                        objectType,
                        midiEvents,
                        match,
                        action,
                        expectedMidiEvents,
                        expectedProcessedCount,
                        settings,
                        hint,
                        newReferencesAllowed);
                    break;
            }
        }

        private void ProcessObjects_EventsCollection(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                eventsCollection.ProcessObjects(objectType, action, match, settings, hint),
                "Invalid count of processed objects for events collection.");

            var expectedEventsCollection = new EventsCollection();
            expectedEventsCollection.AddRange(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events collection is invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents, eventsCollection);
        }

        private void ProcessObjects_TrackChunk(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var trackChunk = new TrackChunk(midiEvents);

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                trackChunk.ProcessObjects(objectType, action, match, settings, hint),
                "Invalid count of processed objects for track chunk.");

            var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Track chunk is invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents, trackChunk.Events);
        }

        private void ProcessObjects_TrackChunks(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                trackChunks.ProcessObjects(objectType, action, match, settings, hint),
                "Invalid count of processed objects for track chunks.");

            MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Track chunks are invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), trackChunks.SelectMany(c => c.Events));
        }

        private void ProcessObjects_File(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<ITimedObject> match,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e)));

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                midiFile.ProcessObjects(objectType, action, match, settings, hint),
                "Invalid count of processed objects for file.");

            MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "File is invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), midiFile.GetTrackChunks().SelectMany(c => c.Events));
        }

        private void ProcessObjects_EventsCollection(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var expectedProcessedCount = eventsCollection.GetObjects(objectType, settings).Count;

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                eventsCollection.ProcessObjects(objectType, action, settings, hint),
                "Invalid count of processed objects for events collection.");

            var expectedEventsCollection = new EventsCollection();
            expectedEventsCollection.AddRange(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events collection is invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents, eventsCollection);
        }

        private void ProcessObjects_TrackChunk(
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var trackChunk = new TrackChunk(midiEvents);
            var expectedProcessedCount = trackChunk.GetObjects(objectType, settings).Count;

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                trackChunk.ProcessObjects(objectType, action, settings, hint),
                "Invalid count of processed objects for track chunk.");

            var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Track chunk is invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents, trackChunk.Events);
        }

        private void ProcessObjects_TrackChunks(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var expectedProcessedCount = trackChunks.GetObjects(objectType, settings).Count;

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                trackChunks.ProcessObjects(objectType, action, settings, hint),
                "Invalid count of processed objects for track chunks.");

            MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Track chunks are invalid.");

            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), trackChunks.SelectMany(c => c.Events));
        }

        private void ProcessObjects_File(
            ObjectType objectType,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings,
            ObjectProcessingHint hint,
            bool newReferencesAllowed)
        {
            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e)));
            var expectedProcessedCount = midiFile.GetObjects(objectType, settings).Count;

            ClassicAssert.AreEqual(
                expectedProcessedCount,
                midiFile.ProcessObjects(objectType, action, settings, hint),
                "Invalid count of processed objects for file.");

            MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "File is invalid.");
            
            if (!newReferencesAllowed)
                CheckNoNewEventsReferences(midiEvents.SelectMany(e => e), midiFile.GetTrackChunks().SelectMany(c => c.Events));
        }

        #endregion
    }
}
