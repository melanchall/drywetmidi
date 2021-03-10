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
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetNotes_EventsCollection_EmptyCollection([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[0],
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NoteOn([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new[] { new NoteOnEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NoteOff([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new[] { new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NonNote([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new[] { new TextEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_Note_1([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_Note_2([Values] ContainerType containerType, [Values(0, 100)] long time, [Values(0, 50)] long length)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = time }, new NoteOffEvent { DeltaTime = length } },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue, length, time) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOn_1([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOn_2([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOff_NoteOff_1([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOff_NoteOff_2([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOff_DifferentIds([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue), new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_SequentialPairs([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)10 },
                    new NoteOffEvent { Channel = (FourBitNumber)10 },
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Time = 100 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 150 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 150 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 150, Channel = (FourBitNumber)10 },
                });
        }

        [Test]
        public void GetNotes_EventsCollection_NotesWithinOne([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                    new NoteOffEvent(),
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 200 },
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_EventsCollection_NotesWithinUncompletedOne_1([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                },
                expectedNotes: new[]
                {
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_EventsCollection_NotesWithinUncompletedOne_2([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
                    new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                    new NoteOffEvent()
                },
                expectedNotes: new[]
                {
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_EventsCollection_OverllappedWithDifferentIds([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                    new NoteOffEvent { DeltaTime = 20 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
                    new TextEvent("B"),
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 20 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 20, Channel = (FourBitNumber)4 },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Length = 20 },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Length = 20, Channel = (FourBitNumber)8 },
                });
        }

        [Test]
        public void GetNotes_EventsCollection_NoteAroundEvents([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent { DeltaTime = 100 },
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 }
                });
        }

        [Test]
        public void GetNotes_EventsCollection_NoteOnBeforeEvents([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_NoteOnAfterEvents([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_NoteOffAfterEvents([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_NoteOffBeforeEvents([Values] ContainerType containerType)
        {
            GetNotes_EventsCollection(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOffEvent(),
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_EmptyCollection([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_SequentialPairs([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 25 },
                        new NoteOnEvent((SevenBitNumber)10, SevenBitNumber.MaxValue) { DeltaTime = 5 },
                        new NoteOffEvent((SevenBitNumber)10, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Channel = (FourBitNumber)5, DeltaTime = 10 },
                        new NoteOffEvent { Channel = (FourBitNumber)5, DeltaTime = 70 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = SevenBitNumber.MaxValue, Length = 25 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Channel = (FourBitNumber)5, Time = 10, Length = 70 },
                    new Note((SevenBitNumber)10) { Velocity = SevenBitNumber.MaxValue, Length = 10, Time = 30 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100, Time = 50 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_NotesWithinUncompletedOne_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_NotesWithinUncompletedOne_2([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                        new NoteOffEvent()
                    }
                },
                expectedNotes: new[]
                {
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_NoteOffsInDifferentTrackChunks([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOnEvent(),
                        new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue)
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 100 },
                        new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue)
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                    new Note((SevenBitNumber)80) { Velocity = SevenBitNumber.MaxValue, Length = 100 }
                });
        }

        [Test]
        public void GetNotes_TrackChunks_OverllappedWithDifferentIds([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOnEvent { Channel = (FourBitNumber)4 },
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 20 },
                        new NoteOffEvent { Channel = (FourBitNumber)4 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 20 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 20, Channel = (FourBitNumber)4 },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Length = 20 },
                    new Note((SevenBitNumber)70) { Velocity = SevenBitNumber.MaxValue, Length = 20, Channel = (FourBitNumber)8 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NoteAroundEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent { DeltaTime = 100 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 }
                });
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NoteOnBeforeEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NoteOnAfterEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOnEvent(),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NoteOffAfterEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NoteOffBeforeEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOffEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_MultipleTrackChunk_NoteAroundEvents_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent { DeltaTime = 100 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 }
                });
        }

        [Test]
        public void GetNotes_TrackChunks_MultipleTrackChunk_NoteAroundEvents_2([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("B"),
                        new NoteOffEvent { DeltaTime = 200 },
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 200 },
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_NoteOnBeforeEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_NoteOnAfterEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOnEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOnEvent(),
                        new NoteOnEvent(),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_NoteOffAfterEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                        new NoteOffEvent(),
                        new NoteOffEvent(),
                    }
                },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_NoteOffBeforeEvents([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOffEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent(),
                        new NoteOffEvent(),
                        new TextEvent("A"),
                        new TextEvent("B"),
                    }
                },
                expectedNotes: new Note[0]);
        }

        #endregion

        #region Private methods

        private void GetNotes_EventsCollection(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            ICollection<Note> expectedNotes)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        var notes = eventsCollection.GetNotes();
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);
                        var notes = trackChunk.GetNotes();
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetNotes_TrackChunks(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            expectedNotes);
                    }
                    break;
            }
        }

        private void GetNotes_TrackChunks(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Note> expectedNotes)
        {
            IEnumerable<Note> notes;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                notes = new MidiFile(trackChunks).GetNotes();
            else
                notes = trackChunks.GetNotes();

            MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
        }

        #endregion
    }
}
