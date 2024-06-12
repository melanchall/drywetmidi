using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Nested classes

        private sealed class CustomTimedEvent_ForNote : TimedEvent
        {
            public CustomTimedEvent_ForNote(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
                : base(midiEvent, time)
            {
                EventsCollectionIndex = eventsCollectionIndex;
                EventIndex = eventIndex;
            }

            public int EventsCollectionIndex { get; }

            public int EventIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent_ForNote(Event, Time, EventsCollectionIndex, EventIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomTimedEvent_ForNote CustomTimedEvent_ForNote))
                    return false;

                return CustomTimedEvent_ForNote.EventsCollectionIndex == EventsCollectionIndex &&
                    CustomTimedEvent_ForNote.EventIndex == EventIndex;
            }
        }

        private sealed class CustomNote : Note
        {
            public CustomNote(TimedEvent noteOnTimedEvent, TimedEvent noteOffTimedEvent, int? eventsCollectionIndex)
                : base(noteOnTimedEvent, noteOffTimedEvent, false)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public CustomNote(SevenBitNumber noteNumber, int? eventsCollectionIndex)
                : base(noteNumber)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int? EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomNote(TimedNoteOnEvent, TimedNoteOffEvent, EventsCollectionIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomNote customNote))
                    return false;

                return customNote.EventsCollectionIndex == EventsCollectionIndex;
            }
        }

        #endregion

        #region Constants

        private static readonly NoteMethods NoteMethods = new NoteMethods();

        private static readonly TimedEventDetectionSettings CustomEventSettings_ForNote = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent_ForNote(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
        };

        private static readonly Func<NoteData, Note> CustomNoteConstructor =
            data => new CustomNote(
                data.TimedNoteOnEvent,
                data.TimedNoteOffEvent,
                (data.TimedNoteOnEvent as CustomTimedEvent_ForNote)?.EventsCollectionIndex);

        #endregion

        #region Test methods

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_1([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                    Constructor = CustomNoteConstructor
                }
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
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_2([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                    Constructor = CustomNoteConstructor,
                    TimedEventDetectionSettings = CustomEventSettings_ForNote
                }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((CustomNote)n).Velocity = (SevenBitNumber)70,
            match: n => ((CustomTimedEvent_ForNote)((Note)n).GetTimedNoteOnEvent()).EventIndex >= 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_1_Custom_3([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                    Constructor = CustomNoteConstructor,
                    TimedEventDetectionSettings = CustomEventSettings_ForNote
                }
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
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_2([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_FirstNoteOn_3([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 120,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_1([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_2([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate_LastNoteOn_3([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 130,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 70 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_1([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_2([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_FirstNoteOn_3([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 30 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: n => ((Note)n).Length -= 40,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 40 },
            });

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_1([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOffEvent(),
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_2([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
        public void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate_LastNoteOn_3([Values] ContainerType containerType) => ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            containerType,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 50 },
                new NoteOffEvent { DeltaTime = 40 },
            },
            action: n => ((Note)n).Length -= 40,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { DeltaTime = 20 },
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 40 },
            });

        [Test]
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1_Custom_1([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                    Constructor = CustomNoteConstructor
                }
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_1_Custom_2([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn,
                    Constructor = CustomNoteConstructor,
                    TimedEventDetectionSettings = CustomEventSettings_ForNote
                }
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
            match: n => ((CustomTimedEvent_ForNote)((Note)n).GetTimedNoteOffEvent()).EventIndex >= 0,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_4([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 100,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_5([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 100,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_6([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 80,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_7([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 80,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_FirstNoteOn_8([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 80,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_1([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_4([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 0,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_5([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_6([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_7([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_8([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate_LastNoteOn_9([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
            match: n => ((Note)n).Length == 130,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_1([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_2([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
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
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                }
            },
            action: n => ((Note)n).OffVelocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_4([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_6([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).Length -= 40,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_FirstNoteOn_7([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn }
            },
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
            action: n => ((Note)n).Length -= 40,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_1([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
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
            action: n => ((Note)n).Velocity = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_4([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).Length += 20,
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
        public void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate_LastNoteOn_7([Values] bool wrapToFile) => ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            wrapToFile,
            new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings { NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }
            },
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
            action: n => ((Note)n).Length -= 30,
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

        #endregion

        #region Private methods

        private void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount) => ProcessObjects_DetectionSettings_EventsCollection_WithPredicate(
                containerType,
                ObjectType.Note,
                settings,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount);

        private void ProcessObjects_Notes_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents) => ProcessObjects_DetectionSettings_EventsCollection_WithoutPredicate(
                containerType,
                ObjectType.Note,
                settings,
                midiEvents,
                action,
                expectedMidiEvents);

        private void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithPredicate(
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount) => ProcessObjects_DetectionSettings_TrackChunks_WithPredicate(
                ObjectType.Note,
                wrapToFile,
                settings,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount);

        private void ProcessObjects_Notes_DetectionSettings_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents) => ProcessObjects_DetectionSettings_TrackChunks_WithoutPredicate(
                ObjectType.Note,
                wrapToFile,
                settings,
                midiEvents,
                action,
                expectedMidiEvents);

        #endregion
    }
}
