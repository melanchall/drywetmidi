using System;
using System.Collections.Generic;
using System.Linq;
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
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_2([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            match: n => n.Length == 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_3([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            match: n => n.Length == 120,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 80 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_1([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_2([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            match: n => n.Length == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 100 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_3([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            match: n => n.Length == 130,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 110 },
                new NoteOffEvent { DeltaTime = 70 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_1([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_2([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_3([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_1([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_2([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_3([Values] ContainerType containerType) => RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 50 },
                new NoteOffEvent { DeltaTime = 40 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_2([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_3([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => n.Length == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_4([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => n.Length == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_5([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            match: n => n.Length == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_6([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_7([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_8([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            match: n => n.Length == 80,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 120 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_1([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_2([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_3([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Length == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_4([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Length == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_5([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            match: n => n.Time == 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_6([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_7([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Length == 70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_8([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
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
            match: n => n.Length == 70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_9([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            match: n => n.Length == 130,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 110 },
                    new NoteOffEvent { DeltaTime = 70 },
                },
                new MidiEvent[]
                {
                }
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_1([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_2([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_3([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_4([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_5([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_6([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 180 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_7([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 30 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 230 },
                }
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_8([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 150 },
                    new NoteOffEvent { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_1([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_2([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_3([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_4([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_5([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_6([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        [Test]
        public void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_7([Values] bool wrapToFile) => RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn },
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { DeltaTime = 50 },
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 180 },
                    new NoteOffEvent { DeltaTime = 50 },
                }
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                }
            });

        #endregion

        #region Private methods

        private void RemoveNotes_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Predicate<Note> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedRemovedCount,
                            eventsCollection.RemoveNotes(match, settings),
                            "Invalid count of removed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedRemovedCount,
                            trackChunk.RemoveNotes(match, settings),
                            "Invalid count of removed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            match,
                            new[] { expectedMidiEvents },
                            expectedRemovedCount);
                    }
                    break;
            }
        }

        private void RemoveNotes_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            NoteDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var notesCount = eventsCollection.GetNotes().Count();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            notesCount,
                            eventsCollection.RemoveNotes(settings),
                            "Invalid count of removed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);

                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            notesCount,
                            trackChunk.RemoveNotes(settings),
                            "Invalid count of removed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void RemoveNotes_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<Note> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveNotes(match, settings),
                    "Invalid count of removed notes.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveNotes(match, settings),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void RemoveNotes_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            NoteDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var notesCount = trackChunks.GetNotes().Count();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    notesCount,
                    midiFile.RemoveNotes(settings),
                    "Invalid count of removed notes.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    notesCount,
                    trackChunks.RemoveNotes(settings),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
