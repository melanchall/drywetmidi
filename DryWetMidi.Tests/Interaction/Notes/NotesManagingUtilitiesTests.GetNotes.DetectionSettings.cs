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
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_FirstNoteOn_1([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_FirstNoteOn_1_Custom_1([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                Constructor = CustomNoteConstructor
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
                new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
            },
            additionalChecks: notes =>
            {
                foreach (var n in notes)
                {
                    ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                    ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                }
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_FirstNoteOn_1_Custom_2([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                Constructor = CustomNoteConstructor,
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new CustomNote(SevenBitNumber.MinValue, 0) { Velocity = SevenBitNumber.MinValue },
                new CustomNote(SevenBitNumber.MinValue, 0) { Velocity = SevenBitNumber.MinValue },
            },
            additionalChecks: notes =>
            {
                foreach (var n in notes)
                {
                    ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                    ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                }
            },
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_FirstNoteOn_2([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_FirstNoteOn_3([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_LastNoteOn_1([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_LastNoteOn_2([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_EventsCollection_LastNoteOn_3([Values] ContainerType containerType) => GetNotes_DetectionSettings_EventsCollection(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_1([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_1_Custom_1([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                Constructor = CustomNoteConstructor
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
                new CustomNote(SevenBitNumber.MinValue, null) { Velocity = SevenBitNumber.MinValue },
            },
            additionalChecks: notes =>
            {
                foreach (var n in notes)
                {
                    ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                    ClassicAssert.IsNotInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                }
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_1_Custom_2([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                Constructor = CustomNoteConstructor,
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new CustomNote(SevenBitNumber.MinValue, 0) { Velocity = SevenBitNumber.MinValue },
                new CustomNote(SevenBitNumber.MinValue, 1) { Velocity = SevenBitNumber.MinValue },
            },
            additionalChecks: notes =>
            {
                foreach (var n in notes)
                {
                    ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOnEvent(), "Invalid Note On timed event type.");
                    ClassicAssert.AreEqual(0, ((CustomTimedEvent)n.GetTimedNoteOnEvent()).EventIndex, "Invalid index of Note On event.");
                    ClassicAssert.IsInstanceOf<CustomTimedEvent>(n.GetTimedNoteOffEvent(), "Invalid Note Off timed event type.");
                    ClassicAssert.AreEqual(1, ((CustomTimedEvent)n.GetTimedNoteOffEvent()).EventIndex, "Invalid index of Note Off event.");
                }
            },
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_4([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_9([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 150 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 50 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_6([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_7([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_FirstNoteOn_8([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_1([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_4([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_5([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_7([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_8([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_LastNoteOn_9([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
            });

        #endregion

        #region Private methods

        private void GetNotes_DetectionSettings_EventsCollection(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<Note> expectedNotes,
            Action<ICollection<Note>> additionalChecks = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        
                        var notes = eventsCollection.GetNotes(settings, timedEventDetectionSettings);
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
                        additionalChecks?.Invoke(notes);

                        var timedObjects = eventsCollection.GetObjects(ObjectType.Note, new ObjectDetectionSettings
                        {
                            NoteDetectionSettings = settings,
                            TimedEventDetectionSettings = timedEventDetectionSettings
                        });
                        MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
                        additionalChecks?.Invoke(timedObjects.Cast<Note>().ToArray());
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        var notes = trackChunk.GetNotes(settings, timedEventDetectionSettings);
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
                        additionalChecks?.Invoke(notes);

                        var timedObjects = trackChunk.GetObjects(ObjectType.Note, new ObjectDetectionSettings
                        {
                            NoteDetectionSettings = settings,
                            TimedEventDetectionSettings = timedEventDetectionSettings
                        });
                        MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
                        additionalChecks?.Invoke(timedObjects.Cast<Note>().ToArray());
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetNotes_DetectionSettings_TrackChunks(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            expectedNotes,
                            additionalChecks,
                            timedEventDetectionSettings);
                    }
                    break;
            }
        }

        private void GetNotes_DetectionSettings_TrackChunks(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Note> expectedNotes,
            Action<ICollection<Note>> additionalChecks = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ICollection<Note> notes;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                notes = new MidiFile(trackChunks).GetNotes(settings, timedEventDetectionSettings);
            else
                notes = trackChunks.GetNotes(settings, timedEventDetectionSettings);

            MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
            additionalChecks?.Invoke(notes);

            //

            IEnumerable<ITimedObject> timedObjects;

            if (wrapToFile)
                timedObjects = new MidiFile(trackChunks).GetObjects(ObjectType.Note, new ObjectDetectionSettings
                {
                    NoteDetectionSettings = settings,
                    TimedEventDetectionSettings = timedEventDetectionSettings
                });
            else
                timedObjects = trackChunks.GetObjects(ObjectType.Note, new ObjectDetectionSettings
                {
                    NoteDetectionSettings = settings,
                    TimedEventDetectionSettings = timedEventDetectionSettings
                });

            MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
            additionalChecks?.Invoke(timedObjects.Cast<Note>().ToArray());
        }

        #endregion
    }
}
