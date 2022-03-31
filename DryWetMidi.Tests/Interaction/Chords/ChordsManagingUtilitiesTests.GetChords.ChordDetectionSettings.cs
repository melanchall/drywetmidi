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
        public void GetChords_DetectionSettings_TrackChunks_AllEventsCollections_NotesTolerance_1([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesTolerance = 0,
                ChordSearchContext = ChordSearchContext.AllEventsCollections,
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteSearchContext = NoteSearchContext.AllEventsCollections
                }
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
            });

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
        public void GetChords_DetectionSettings_TrackChunks_AllEventsCollections_NotesTolerance_2([Values] bool wrapToFile, [Values(1, 10)] int notesTolerance) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesTolerance = 10,
                ChordSearchContext = ChordSearchContext.AllEventsCollections,
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteSearchContext = NoteSearchContext.AllEventsCollections
                }
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
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
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Time = notesTolerance - 1 })
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
        public void GetChords_DetectionSettings_TrackChunks_AllEventsCollections_NotesMinCount_1([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesMinCount = 1,
                ChordSearchContext = ChordSearchContext.AllEventsCollections,
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteSearchContext = NoteSearchContext.AllEventsCollections
                }
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
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
        public void GetChords_DetectionSettings_TrackChunks_AllEventsCollections_NotesMinCount_2([Values] bool wrapToFile) => GetChords_DetectionSettings_TrackChunks(
            wrapToFile,
            new ChordDetectionSettings
            {
                NotesMinCount = 2,
                ChordSearchContext = ChordSearchContext.AllEventsCollections,
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteSearchContext = NoteSearchContext.AllEventsCollections
                }
            },
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
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("D"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                }
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue })
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

        #endregion

        #region Private methods

        private void GetChords_DetectionSettings_EventsCollection(
            ContainerType containerType,
            ChordDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<Chord> expectedChords)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        
                        var chords = eventsCollection.GetChords(settings);
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

                        var timedObjets = eventsCollection.GetObjects(ObjectType.Chord, new ObjectDetectionSettings { ChordDetectionSettings = settings });
                        MidiAsserts.AreEqual(expectedChords, timedObjets, "Chords are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);
                        
                        var chords = trackChunk.GetChords(settings);
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

                        var timedObjets = trackChunk.GetObjects(ObjectType.Chord, new ObjectDetectionSettings { ChordDetectionSettings = settings });
                        MidiAsserts.AreEqual(expectedChords, timedObjets, "Chords are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetChords_DetectionSettings_TrackChunks(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            expectedChords);
                    }
                    break;
            }
        }

        private void GetChords_DetectionSettings_TrackChunks(
            bool wrapToFile,
            ChordDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Chord> expectedChords)
        {
            IEnumerable<Chord> chords;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                chords = new MidiFile(trackChunks).GetChords(settings);
            else
                chords = trackChunks.GetChords(settings);

            MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

            //

            IEnumerable<ITimedObject> timedObjects;

            if (wrapToFile)
                timedObjects = new MidiFile(trackChunks).GetObjects(ObjectType.Chord, new ObjectDetectionSettings { ChordDetectionSettings = settings });
            else
                timedObjects = trackChunks.GetObjects(ObjectType.Chord, new ObjectDetectionSettings { ChordDetectionSettings = settings });

            MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
        }

        #endregion
    }
}
