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
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetNotes_EventsCollection_EmptyCollection([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NoteOn([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NoteOff([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_OneEvent_NonNote([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new[] { new TextEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_Note_1([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_Note_2([Values] bool wrapToTrackChunk, [Values(0, 100)] long time, [Values(0, 50)] long length)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = time }, new NoteOffEvent { DeltaTime = length } },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue, length, time) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOn_1([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOn_2([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOff_NoteOff_1([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOff_NoteOff_2([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_TwoEvents_NoteOn_NoteOff_DifferentIds([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue), new NoteOffEvent() },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_EventsCollection_SequentialPairs([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
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
        public void GetNotes_EventsCollection_NotesWithinOne([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
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
        public void GetNotes_EventsCollection_NotesWithinUncompletedOne_1([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
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
        public void GetNotes_EventsCollection_NotesWithinUncompletedOne_2([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
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
        public void GetNotes_EventsCollection_OverllappedWithDifferentIds([Values] bool wrapToTrackChunk)
        {
            GetNotes_EventsCollection(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                    new NoteOffEvent { DeltaTime = 20 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
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
        public void GetNotes_TrackChunks_EmptyCollection([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_OneEvent_NoteOn([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_OneEvent_NoteOff([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOffEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_OneEvent_NonNote([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new[] { new TextEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_Note_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() } },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_Note_2([Values] bool wrapToFile, [Values(0, 100)] long time, [Values(0, 50)] long length)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent { DeltaTime = time }, new NoteOffEvent { DeltaTime = length } } },
                expectedNotes: new[] { new Note(SevenBitNumber.MinValue, length, time) { Velocity = SevenBitNumber.MinValue } });
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_NoteOn_NoteOn_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_NoteOn_NoteOn_2([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_NoteOff_NoteOff_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_NoteOff_NoteOff_2([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOffEvent(), new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_TwoEvents_NoteOn_NoteOff_DifferentIds([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MaxValue), new NoteOffEvent() } },
                expectedNotes: new Note[0]);
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_SequentialPairs([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
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
                    }
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
        public void GetNotes_TrackChunks_OneTrackChunk_NotesWithinOne([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                        new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)7, (SevenBitNumber)10) { DeltaTime = 150 },
                        new NoteOffEvent(),
                    }
                },
                expectedNotes: new[]
                {
                    new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 200 },
                    new Note((SevenBitNumber)70) { Velocity = (SevenBitNumber)10, Length = 50 },
                    new Note((SevenBitNumber)7) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)10, Time = 50, Length = 150 },
                });
        }

        [Test]
        public void GetNotes_TrackChunks_OneTrackChunk_NotesWithinUncompletedOne_1([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                        new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
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
        public void GetNotes_TrackChunks_OneTrackChunk_NotesWithinUncompletedOne_2([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                        new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)10),
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
        public void GetNotes_TrackChunks_OneTrackChunk_OverllappedWithDifferentIds([Values] bool wrapToFile)
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
        public void GetNotes_TrackChunks_MultipleTrackChunks_SequentialPairs([Values] bool wrapToFile)
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
        public void GetNotes_TrackChunks_MultipleTrackChunks_NotesWithinUncompletedOne_1([Values] bool wrapToFile)
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
        public void GetNotes_TrackChunks_MultipleTrackChunks_NotesWithinUncompletedOne_2([Values] bool wrapToFile)
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
        public void GetNotes_TrackChunks_MultipleTrackChunks_NoteOffsInDifferentTrackChunks([Values] bool wrapToFile)
        {
            GetNotes_TrackChunks(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
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
        public void GetNotes_TrackChunks_MultipleTrackChunks_OverllappedWithDifferentIds([Values] bool wrapToFile)
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

        #endregion

        #region Private methods

        private void GetNotes_EventsCollection(
            bool wrapToTrackChunk,
            IEnumerable<MidiEvent> midiEvents,
            IEnumerable<Note> expectedNotes)
        {
            IEnumerable<Note> notes;

            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);
                notes = trackChunk.GetNotes();
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);
                notes = eventsCollection.GetNotes();
            }

            MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");
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
