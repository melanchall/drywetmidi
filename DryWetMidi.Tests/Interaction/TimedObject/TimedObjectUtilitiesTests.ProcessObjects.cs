using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessObjects_TimedEventsAndNotes_WithPredicate_1([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithPredicate(
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
            match: obj => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("B"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessObjects_TimedEventsAndNotes_WithPredicate_2([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithPredicate(
            containerType: containerType,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
            },
            action: obj => obj.Time += 10,
            match: obj => obj is TimedEvent timedEvent && timedEvent.Event is TextEvent textEvent && textEvent.Text == "A",
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new TextEvent("A") { DeltaTime = 10 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_1([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithPredicate(
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
            match: obj => true,
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
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_2([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithPredicate(
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
            match: obj => true,
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
        public void ProcessObjects_TimedEventsAndChords_WithPredicate_3([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithPredicate(
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
            action: obj => obj.Time += 10,
            match: obj => obj is Chord,
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
        public void ProcessObjects_TimedEventsAndNotes_WithoutPredicate([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_TimedEventsAndChords_WithoutPredicate_1([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_TimedEventsAndChords_WithoutPredicate_2([Values] ContainerType containerType) => ProcessObjects_EventsCollection_WithoutPredicate(
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

        #endregion
    }
}
