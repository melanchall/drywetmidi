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
        public void GetChords_EventsCollection_EmptyCollection([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[0],
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_EventsCollection_NoNotes([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_EventsCollection_NoChords([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_EventsCollection_OneNote_1([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = (SevenBitNumber)0 })
            });

        [Test]
        public void GetChords_EventsCollection_OneNote_2([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = (SevenBitNumber)0 })
            });

        [Test]
        public void GetChords_EventsCollection_OneNote_3([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = (SevenBitNumber)0 })
            });

        [Test]
        public void GetChords_EventsCollection_OneNote_4([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
                new TextEvent("B"),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = (SevenBitNumber)0 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_1([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)7 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_2([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)7 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_3([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)7 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_4([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("B"),
                new TextEvent("C"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("D"),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)7 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_5([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("B"),
                new TextEvent("C"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent(),
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)7 })
            });

        [Test]
        public void GetChords_EventsCollection_MultipleNotes_6([Values] ContainerType containerType) => GetChords_EventsCollection(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 100 },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 10 }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 20, Length = 100 }),
            });

        [Test]
        public void GetChords_EventsCollection_AllNotesOnSingleChannel([Values] ContainerType containerType)
        {
            const int chordsCount = 100;

            GetChords_EventsCollection(
                containerType,
                midiEvents: Enumerable.Range(0, chordsCount)
                    .SelectMany(i =>
                    {
                        var events = SevenBitNumber.Values.SelectMany(noteNumber => new MidiEvent[] { new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue), new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) }).ToArray();
                        events[0].DeltaTime = 100;
                        return events;
                    })
                    .ToArray(),
                expectedChords: Enumerable.Range(0, chordsCount)
                    .Select(i => new Chord(SevenBitNumber.Values.Select(noteNumber => new Note(noteNumber) { Velocity = SevenBitNumber.MaxValue }), 100 * (i + 1)))
                    .ToArray());
        }

        [Test]
        public void GetChords_EventsCollection_AllNotesOnAllChannels([Values] ContainerType containerType)
        {
            const int chordsCount = 50;

            GetChords_EventsCollection(
                containerType,
                midiEvents: Enumerable.Range(0, chordsCount)
                    .SelectMany(i =>
                    {
                        var events = FourBitNumber.Values
                            .SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new MidiEvent[]
                            {
                                new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel },
                                new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }
                            }))
                            .ToArray();
                        events[0].DeltaTime = 100;
                        return events;
                    })
                    .ToArray(),
                expectedChords: Enumerable.Range(0, chordsCount)
                    .SelectMany(i => FourBitNumber.Values.Select(channel => new Chord(SevenBitNumber.Values.Select(noteNumber => new Note(noteNumber) { Velocity = SevenBitNumber.MaxValue, Channel = channel }), 100 * (i + 1))))
                    .ToArray());
        }

        [Test]
        public void GetChords_TrackChunks_EmptyCollection([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_TrackChunks_EmptyTrackChunks([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_TrackChunks_NoNotes([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new ControlChangeEvent()
                },
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_TrackChunks_NoChords([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                    new NoteOnEvent { DeltaTime = 100 }
                },
            },
            expectedChords: new Chord[0]);

        [Test]
        public void GetChords_TrackChunks_OneNote_1([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue, 100) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_OneNote_2([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue, 100) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_OneNote_3([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new TextEvent("A"),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue, 100, 50) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_OneNote_4([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new TextEvent("A"),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("B"),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue, 100, 50) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_OneNote_5([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new ControlChangeEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_OneNote_6([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_MultipleNotes_1([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 1000 },
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 100, Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 1000, Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_MultipleNotes_2([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent() { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 1000 },
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 100, Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 1000, Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_MultipleNotes_3([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOnEvent() { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 1000 },
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 1000, Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_MultipleNotes_4([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOnEvent() { Channel = (FourBitNumber)4 },
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 1000 },
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 1000, Velocity = SevenBitNumber.MinValue }),
            });

        [Test]
        public void GetChords_TrackChunks_MultipleNotes_5([Values] bool wrapToFile) => GetChords_TrackChunks(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOnEvent() { Channel = (FourBitNumber)4 },
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 1000 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }),
                new Chord(new Note(SevenBitNumber.MinValue) { Time = 1000, Velocity = SevenBitNumber.MinValue }),
            });

        #endregion

        #region Private methods

        private void GetChords_EventsCollection(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            ICollection<Chord> expectedChords)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        
                        var chords = eventsCollection.GetChords();
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

                        var timedObjects = eventsCollection.GetObjects(ObjectType.Chord);
                        MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);
                        
                        var chords = trackChunk.GetChords();
                        MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

                        var timedObjects = trackChunk.GetObjects(ObjectType.Chord);
                        MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetChords_TrackChunks(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            expectedChords);
                    }
                    break;
            }
        }

        private void GetChords_TrackChunks(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Chord> expectedChords)
        {
            IEnumerable<Chord> chords;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                chords = new MidiFile(trackChunks).GetChords();
            else
                chords = trackChunks.GetChords();

            MidiAsserts.AreEqual(expectedChords, chords, "Chords are invalid.");

            //

            IEnumerable<ITimedObject> timedObjects;

            if (wrapToFile)
                timedObjects = new MidiFile(trackChunks).GetObjects(ObjectType.Chord);
            else
                timedObjects = trackChunks.GetObjects(ObjectType.Chord);

            MidiAsserts.AreEqual(expectedChords, timedObjects, "Chords are invalid from GetObjects.");
        }

        #endregion
    }
}
