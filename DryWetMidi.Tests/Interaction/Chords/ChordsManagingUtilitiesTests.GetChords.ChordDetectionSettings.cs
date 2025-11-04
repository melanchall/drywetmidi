using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class ChordsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_EmptyCollection([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[0],
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_1([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 0 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_1_Custom_1([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                        new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null)
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsNotInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_1_Custom_2([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, null) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null)
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            },
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = CustomNoteConstructor
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_1_Custom_3([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, 0) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, 0) { Velocity = SevenBitNumber.MaxValue }
                    },
                    0)
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            },
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = CustomNoteConstructor,
            },
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_2([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = notesTolerance - 1 },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance - 1 })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_3([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = notesTolerance + 1 },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance + 1 })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_4([Values] ContainerType containerType, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = notesTolerance },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesTolerance_5([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 },
                    new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_1([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_2([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_3([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("B"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_4([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)1 },
                new NoteOffEvent { Channel = (FourBitNumber)1 },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)1 })
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_5([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)1 },
                new NoteOffEvent { Channel = (FourBitNumber)1 },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
            });

        [Test]
        public void GetChords_DetectionSettings_EventsCollection_NotesMinCount_6([Values] ContainerType containerType) => GetChords_DetectionSettings_EventsCollection(
            containerType,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)1 },
                new NoteOffEvent { Channel = (FourBitNumber)1 },
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_EmptyCollection([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new MidiEvent[0][],
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_1([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesTolerance = 0 },
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
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }) { Time = 100 }
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_1_Custom_1([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor
            },
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
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                        new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null),
                new CustomChord(
                    new[]
                    {
                        new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                        new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null) { Time = 100 }
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsNotInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_1_Custom_2([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor,
            },
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
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, null) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null),
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, null) { Velocity = SevenBitNumber.MaxValue }
                    },
                    null) { Time = 100 }
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            },
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = CustomNoteConstructor
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_1_Custom_3([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                Constructor = CustomChordConstructor,
            },
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
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, 0) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, 0) { Velocity = SevenBitNumber.MaxValue }
                    },
                    0),
                new CustomChord(
                    new[]
                    {
                        new CustomNote(SevenBitNumber.MinValue, 1) { Velocity = SevenBitNumber.MinValue },
                        new CustomNote((SevenBitNumber)70, 1) { Velocity = SevenBitNumber.MaxValue }
                    },
                    1) { Time = 100 }
            },
            additionalChecks: chords =>
            {
                foreach (var c in chords)
                {
                    foreach (var n in c.Notes)
                    {
                        ClassicAssert.IsInstanceOf<CustomNote>(n, "Invalid note type.");

                        ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                        ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    }
                }
            },
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = CustomNoteConstructor,
            },
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_2([Values] bool wrapToFile, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesTolerance = 10 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = notesTolerance - 1 },
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("E"),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance - 1 }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesTolerance_3([Values] bool wrapToFile, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesTolerance = notesTolerance },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = notesTolerance + 1 },
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("E"),
                }
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance + 1 })
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_1([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_2([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new TextEvent("E"),
                    new NoteOnEvent(),
                    new TextEvent("F"),
                    new NoteOffEvent(),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_3([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("E"),
                }
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_4([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 1 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent { Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)1 }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_5([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 2 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 },
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
            });

        [Test]
        public void GetChords_DetectionSettings_TrackChunks_NotesMinCount_6([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings { NotesMinCount = 3 },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent { Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 },
                }
            },
            expectedChords: new Chord[0]);

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_1() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: new ChordDetectionSettings
            {
                Constructor = chordData => null,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_2() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: new ChordDetectionSettings
            {
                Constructor = chordData => chordData.Notes.First().Time == 0
                    ? null
                    : new Chord(chordData.Notes),
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_3() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: null,
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = noteData => null,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_4() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: null,
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = noteData => noteData.TimedNoteOnEvent.Time == 0
                    ? null
                    : new Note(noteData.TimedNoteOnEvent, noteData.TimedNoteOffEvent),
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_5() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: null,
            noteDetectionSettings: null,
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = timedEventData => null,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        // TODO: describe in docs
        [Test]
        public void GetChords_Custom_Null_6() => GetChords_DetectionSettings_EventsCollection(
            containerType: ContainerType.EventsCollection,
            settings: null,
            noteDetectionSettings: null,
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = timedEventData => timedEventData.Time == 0
                    ? null
                    : new TimedEvent(timedEventData.Event, timedEventData.Time),
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 }),
            });

        #endregion

        #region Private methods

        private void GetChords_DetectionSettings_EventsCollection(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<Chord> expectedChords,
            Action<ICollection<Chord>> additionalChecks = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        
                        var chords = eventsCollection.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");
                        additionalChecks?.Invoke(chords);

                        var timedObjects = eventsCollection.GetObjects(ObjectType.Chord, new ObjectDetectionSettings
                        {
                            ChordDetectionSettings = settings,
                            NoteDetectionSettings = noteDetectionSettings,
                            TimedEventDetectionSettings = timedEventDetectionSettings,
                        });
                        MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
                        additionalChecks?.Invoke(timedObjects.Cast<Chord>().ToArray());
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);
                        
                        var chords = trackChunk.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");
                        additionalChecks?.Invoke(chords);

                        var timedObjects = trackChunk.GetObjects(ObjectType.Chord, new ObjectDetectionSettings
                        {
                            ChordDetectionSettings = settings,
                            NoteDetectionSettings = noteDetectionSettings,
                            TimedEventDetectionSettings = timedEventDetectionSettings,
                        });
                        MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
                        additionalChecks?.Invoke(timedObjects.Cast<Chord>().ToArray());
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetChords_DetectionSettings_TrackChunks(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            expectedChords,
                            additionalChecks,
                            noteDetectionSettings,
                            timedEventDetectionSettings);
                    }
                    break;
            }
        }

        private void GetChords_DetectionSettings_TrackChunks(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Chord> expectedChords,
            Action<ICollection<Chord>> additionalChecks = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ICollection<Chord> chords;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                chords = new MidiFile(trackChunks).GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
            else
                chords = trackChunks.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);

            MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");
            additionalChecks?.Invoke(chords);

            //

            IEnumerable<ITimedObject> timedObjects;

            if (wrapToFile)
                timedObjects = new MidiFile(trackChunks).GetObjects(ObjectType.Chord, new ObjectDetectionSettings
                {
                    ChordDetectionSettings = settings,
                    NoteDetectionSettings = noteDetectionSettings,
                    TimedEventDetectionSettings = timedEventDetectionSettings,
                });
            else
                timedObjects = trackChunks.GetObjects(ObjectType.Chord, new ObjectDetectionSettings
                {
                    ChordDetectionSettings = settings,
                    NoteDetectionSettings = noteDetectionSettings,
                    TimedEventDetectionSettings = timedEventDetectionSettings,
                });

            MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
            additionalChecks?.Invoke(timedObjects.Cast<Chord>().ToArray());
        }

        #endregion
    }
}
