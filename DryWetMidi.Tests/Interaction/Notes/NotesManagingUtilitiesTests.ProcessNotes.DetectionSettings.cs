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
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_1([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
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
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_2([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
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
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => ((CustomTimedEvent)n.GetTimedNoteOnEvent()).EventIndex >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2,
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_3([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
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
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => ((CustomNote)n).EventsCollectionIndex >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2,
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_2([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_3([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 120,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_1([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_2([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_3([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 130,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_1([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_2([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_3([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => n.Length -= 40,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 40 },
            });

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_1([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_2([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => n.Time += 20,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_3([Values] ContainerType containerType) => ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 50 },
                new NoteOffEvent { DeltaTime = 40 },
            },
            action: n => n.Length -= 40,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 40 },
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1_Custom_1([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1_Custom_2([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => ((CustomTimedEvent)n.GetTimedNoteOffEvent()).EventIndex >= 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 3,
            timedEventDetectionSettings: CustomEventSettings);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_4([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_5([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Length == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_6([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_7([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70, Velocity = (SevenBitNumber)70 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_8([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 80 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 80, Velocity = (SevenBitNumber)70 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_1([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_4([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_5([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Time == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_6([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                }
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_7([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            match: n => n.Length == 70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_8([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Length == 70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 70, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_9([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 70 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.OffVelocity = (SevenBitNumber)70,
            match: n => n.Length == 130,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 70 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50, Velocity = (SevenBitNumber)70 },
                }
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_1([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => n.Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_2([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => n.OffVelocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_4([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            action: n => n.Time += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 120 },
                    new NoteOffEvent(),
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_6([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            action: n => n.Length -= 40,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 60 },
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_7([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.Length -= 40,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_1([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
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
            action: n => n.Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_4([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
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
            action: n => n.Length += 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 20 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 20 },
                }
            });

        [Test]
        public void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_7([Values] bool wrapToFile) => ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 70 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            action: n => n.Length -= 30,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 40 },
                    new NoteOffEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 20 },
                }
            });

        // TODO: describe in docs
        [Test]
        public void ProcessNotes_Custom_Null_1() => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType: ContainerType.EventsCollection,
            settings: new NoteDetectionSettings
            {
                Constructor = noteData => null
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            action: note => note.Length = 100,
            match: note => note.Time >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOnEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            expectedProcessedCount: 2);

        // TODO: describe in docs
        [Test]
        public void ProcessNotes_Custom_Null_2() => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType: ContainerType.EventsCollection,
            settings: new NoteDetectionSettings
            {
                Constructor = noteData => noteData.TimedNoteOnEvent.Time == 0
                    ? null
                    : new Note(noteData.TimedNoteOnEvent, noteData.TimedNoteOffEvent)
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            action: note => note.Length = 100,
            match: note => note.Time >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOnEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            expectedProcessedCount: 2);

        // TODO: describe in docs
        [Test]
        public void ProcessNotes_Custom_Null_3() => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType: ContainerType.EventsCollection,
            settings: null,
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = timedEventData => null
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            action: note => note.Length = 100,
            match: note => note.Time >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOnEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            expectedProcessedCount: 2);

        // TODO: describe in docs
        [Test]
        public void ProcessNotes_Custom_Null_4() => ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType: ContainerType.EventsCollection,
            settings: null,
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = timedEventData => timedEventData.Time == 0
                    ? null
                    : new TimedEvent(timedEventData.Event, timedEventData.Time),
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            action: note => note.Length = 100,
            match: note => note.Time >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 0 },
                new NoteOnEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 30 },
            },
            expectedProcessedCount: 2);

        #endregion

        #region Private methods

        private void ProcessNotes_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<Note> action,
            Predicate<Note> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        ClassicAssert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessNotes(action, match, settings, timedEventDetectionSettings),
                            "Invalid count of processed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        ClassicAssert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        ClassicAssert.AreEqual(
                            expectedProcessedCount,
                            trackChunk.ProcessNotes(action, match, settings, timedEventDetectionSettings),
                            "Invalid count of processed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        ClassicAssert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount,
                            timedEventDetectionSettings);
                    }
                    break;
            }
        }

        private void ProcessNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<Note> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var notesCount = midiEvents.GetNotes().Count();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        ClassicAssert.AreEqual(
                            notesCount,
                            eventsCollection.ProcessNotes(action, settings),
                            "Invalid count of processed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        ClassicAssert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        ClassicAssert.AreEqual(
                            notesCount,
                            trackChunk.ProcessNotes(action, settings),
                            "Invalid count of processed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        ClassicAssert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void ProcessNotes_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Note> action,
            Predicate<Note> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessNotes(action, match, settings, timedEventDetectionSettings),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                ClassicAssert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessNotes(action, match, settings, timedEventDetectionSettings),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Note> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var notesCount = trackChunks.GetNotes(settings).Count();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                ClassicAssert.AreEqual(
                    notesCount,
                    midiFile.ProcessNotes(action, settings),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                ClassicAssert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                ClassicAssert.AreEqual(
                    notesCount,
                    trackChunks.ProcessNotes(action, settings),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
