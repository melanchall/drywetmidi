using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Nested classes

        private sealed class CustomTimedEvent_ForChord : TimedEvent
        {
            public CustomTimedEvent_ForChord(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
                : base(midiEvent, time)
            {
                EventsCollectionIndex = eventsCollectionIndex;
                EventIndex = eventIndex;
            }

            public int EventsCollectionIndex { get; }

            public int EventIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent_ForChord(Event, Time, EventsCollectionIndex, EventIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomTimedEvent_ForChord CustomTimedEvent_ForChord))
                    return false;

                return CustomTimedEvent_ForChord.EventsCollectionIndex == EventsCollectionIndex &&
                    CustomTimedEvent_ForChord.EventIndex == EventIndex;
            }
        }

        private sealed class CustomChord : Chord
        {
            public CustomChord(ICollection<Note> notes, int? eventsCollectionIndex)
                : base(notes)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int? EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomChord(Notes.Select(n => (Note)n.Clone()).ToArray(), EventsCollectionIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomChord customChord))
                    return false;

                return customChord.EventsCollectionIndex == EventsCollectionIndex;
            }
        }

        #endregion

        #region Constants

        private static readonly TimedEventDetectionSettings CustomEventSettings_ForChord = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent_ForChord(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
        };

        private static readonly Func<NoteData, Note> CustomNoteConstructor_ForChord =
            data => new CustomNote(
                data.TimedNoteOnEvent,
                data.TimedNoteOffEvent,
                (data.TimedNoteOnEvent as CustomTimedEvent_ForChord)?.EventsCollectionIndex);

        private static readonly Func<ChordData, Chord> CustomChordConstructor =
            data => new CustomChord(
                data.Notes,
                (data.Notes.FirstOrDefault() as CustomNote)?.EventsCollectionIndex);

        #endregion

        #region Test methods

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 }
            },
            midiEvents: new MidiEvent[0],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = 10 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_1_Custom_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((CustomChord)c).Time = 10,
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_1_Custom_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        Constructor = CustomNoteConstructor_ForChord
                    }
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((CustomChord)c).Time = 10,
            match: c => ((CustomNote)((Chord)c).Notes.First()).EventsCollectionIndex != null,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = 10 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_2_Custom([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        Constructor = CustomNoteConstructor_ForChord,
                        TimedEventDetectionSettings = CustomEventSettings_ForChord
                    }
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 10,
            match: c => ((CustomTimedEvent_ForChord)((CustomNote)((CustomChord)c).Notes.First()).GetTimedNoteOnEvent()).EventsCollectionIndex == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_3([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = 0 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_5([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesTolerance_6([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_3([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_4([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_5([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_6([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 3 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate_NotesMinCount_7([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2, NotesTolerance = 10 }
            },
            midiEvents: new MidiEvent[0],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = 10 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1_Custom_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((CustomChord)c).Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1_Custom_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        Constructor = CustomNoteConstructor_ForChord
                    }
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((CustomNote)((Chord)c).Notes.First()).Time = 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_1_Custom_3([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    Constructor = CustomChordConstructor,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        Constructor = CustomNoteConstructor_ForChord,
                        TimedEventDetectionSettings = CustomEventSettings_ForChord
                    }
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c =>
            {
                var note = ((CustomChord)c).Notes.First();
                ((CustomTimedEvent_ForChord)note.TimedNoteOnEvent).Time = 10;
                ((CustomTimedEvent_ForChord)note.TimedNoteOffEvent).Time = 10;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = 0 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_3([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesTolerance = notesTolerance }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_1([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_2([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            },
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_3([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_4([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 3 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate_NotesMinCount_5([Values] ContainerType containerType) => ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 2 }
            },
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
            action: c => ((Chord)c).Channel = (FourBitNumber)8,
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

        private void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount) => ProcessObjects_DetectionSettings_EventsCollection_WithPredicate(
                containerType,
                ObjectType.Chord,
                settings,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount);

        private void ProcessObjects_Chords_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents) => ProcessObjects_DetectionSettings_EventsCollection_WithoutPredicate(
                containerType,
                ObjectType.Chord,
                settings,
                midiEvents,
                action,
                expectedMidiEvents);

        private void ProcessObjects_Chords_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount) => ProcessObjects_DetectionSettings_TrackChunks_WithPredicate(
                ObjectType.Chord,
                wrapToFile,
                settings,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount);

        private void ProcessObjects_Chords_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents) => ProcessObjects_DetectionSettings_TrackChunks_WithoutPredicate(
                ObjectType.Chord,
                wrapToFile,
                settings,
                midiEvents,
                action,
                expectedMidiEvents);

        #endregion
    }
}
