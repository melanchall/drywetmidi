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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_1([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_2([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_3([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_10([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
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
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 50 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 50 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_4([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_5([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_6([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 180 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_7([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            new[]
            {
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_8([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 120 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_FirstNoteOn_9([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 80 },
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_1([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_2([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_3([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent(),
                }
            },
            new[]
            {
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_4([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_5([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100 },
            });

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_6([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Length = 100 },
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_7([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 180 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
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
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_8([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            new[]
            {
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

        [Test]
        public void GetNotes_DetectionSettings_TrackChunks_AllEventsCollections_LastNoteOn_9([Values] bool wrapToFile) => GetNotes_DetectionSettings_TrackChunks(
            wrapToFile,
            new NoteDetectionSettings
            {
                NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn,
                NoteSearchContext = NoteSearchContext.AllEventsCollections
            },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            new[]
            {
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 100, Length = 130 },
                new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue, Time = 110, Length = 70 },
            });

        #endregion

        #region Private methods

        private void GetNotes_DetectionSettings_EventsCollection(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<Note> expectedNotes)
        {
            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);
                        
                        var notes = eventsCollection.GetNotes(settings);
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");

                        var timedObjects = eventsCollection.GetObjects(ObjectType.Note, new ObjectDetectionSettings { NoteDetectionSettings = settings });
                        MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        var notes = trackChunk.GetNotes(settings);
                        MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");

                        var timedObjects = trackChunk.GetObjects(ObjectType.Note, new ObjectDetectionSettings { NoteDetectionSettings = settings });
                        MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        GetNotes_DetectionSettings_TrackChunks(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            expectedNotes);
                    }
                    break;
            }
        }

        private void GetNotes_DetectionSettings_TrackChunks(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            IEnumerable<Note> expectedNotes)
        {
            IEnumerable<Note> notes;

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToArray();

            if (wrapToFile)
                notes = new MidiFile(trackChunks).GetNotes(settings);
            else
                notes = trackChunks.GetNotes(settings);

            MidiAsserts.AreEqual(expectedNotes, notes, "Notes are invalid.");

            //

            // TODO: Support events collection indicies in GetObjects
            if (settings.NoteSearchContext == NoteSearchContext.AllEventsCollections)
            {
                IEnumerable<ITimedObject> timedObjects;

                if (wrapToFile)
                    timedObjects = new MidiFile(trackChunks).GetObjects(ObjectType.Note, new ObjectDetectionSettings { NoteDetectionSettings = settings });
                else
                    timedObjects = trackChunks.GetObjects(ObjectType.Note, new ObjectDetectionSettings { NoteDetectionSettings = settings });

                MidiAsserts.AreEqual(expectedNotes, timedObjects, "Notes are invalid from GetObjects.");
            }
        }

        #endregion
    }
}
