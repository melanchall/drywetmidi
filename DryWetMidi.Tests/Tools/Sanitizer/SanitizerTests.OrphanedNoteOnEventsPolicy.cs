using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_NoNoteEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_NoNoteEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_NoOrphaned_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_NoOrphaned_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOffEvent() { DeltaTime = 35 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 15 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Remove_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Ignore_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.Ignore,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Ignore_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.Ignore,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Ignore_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                },
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.Ignore,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Ignore_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.Ignore,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_Ignore_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.Ignore,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },    // 10
                    new TextEvent("A") { DeltaTime = 50 },   // 60
                    new NoteOnEvent() { DeltaTime = 100 },   // 160
                    new NoteOffEvent() { DeltaTime = 20 })), // 180
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)100,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },     // 10
                    new TextEvent("A") { DeltaTime = 50 },    // 60
                    new NoteOffEvent() { DeltaTime = 50 },    // 110
                    new NoteOnEvent() { DeltaTime = 50 },     // 160
                    new NoteOffEvent() { DeltaTime = 20 }))); // 180

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },    // 10
                    new TextEvent("A") { DeltaTime = 50 },   // 60
                    new NoteOnEvent() { DeltaTime = 100 },   // 160
                    new NoteOffEvent() { DeltaTime = 20 })), // 180
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)200,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },     // 10
                    new TextEvent("A") { DeltaTime = 50 },    // 60
                    new NoteOffEvent() { DeltaTime = 100 },    // 160
                    new NoteOnEvent() { DeltaTime = 0 },     // 160
                    new NoteOffEvent() { DeltaTime = 20 }))); // 180

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_3() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap.Default),
                    new Note((SevenBitNumber)0)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap.Default)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap.Default),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = new MetricTimeSpan(0, 0, 0, 500),
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap.Default),
                    new Note((SevenBitNumber)0)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap.Default)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1300), TempoMap.Default),
                }.ToTrackChunk()));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_4() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap.Default),
                    new Note((SevenBitNumber)0)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap.Default)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap.Default),
                }.ToTrackChunk(),
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1000), TempoMap.Default),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = new MetricTimeSpan(0, 0, 0, 500),
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 200), TempoMap.Default),
                    new Note((SevenBitNumber)0)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 250), TempoMap.Default)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 500), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1000), TempoMap.Default),
                }.ToTrackChunk(),
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"))
                        .SetTime(new MetricTimeSpan(), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 800), TempoMap.Default),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1000), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1100), TempoMap.Default),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 })
                        .SetTime(new MetricTimeSpan(0, 0, 0, 1500), TempoMap.Default),
                }.ToTrackChunk()));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_5() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 200),
                    new Note((SevenBitNumber)0, 500, 250),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 800),
                }.ToTrackChunk(),
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOnEvent(), 600),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 1000),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)500,
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 200),
                    new Note((SevenBitNumber)0, 500, 250),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 800),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 1000),
                }.ToTrackChunk(),
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOnEvent(), 600),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 800),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 1000),
                    new TimedEvent(new NoteOffEvent(), 1100),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 1500),
                }.ToTrackChunk()));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_6() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 300),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 500),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)220,
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 300),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 300),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)2 }, 420),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 500),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 520),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, 620),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)2 }, 720),
                }.ToTrackChunk()));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_7() => Assert.Throws<InvalidOperationException>(
            () => new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 300),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }, 400),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 500),
                }.ToTrackChunk())
                .Sanitize(new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                }),
            "No exception thrown.");

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_8() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 200),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }, 200),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)220,
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)4 }, 200),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }, 200),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)2 }, 420),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)4 }, 420),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, 420),
                }.ToTrackChunk()));

        [Test]
        public void Sanitize_OrphanedNoteOnEventsPolicy_CompleteNote_9() => Sanitize(
            midiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 200),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 205),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 250),
                }.ToTrackChunk()),
            settings: new SanitizingSettings
            {
                OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                NoteMaxLengthForOrphanedNoteOnEvent = (MidiTimeSpan)100,
                NoteMinLength = (MidiTimeSpan)10,
            },
            expectedMidiFile: new MidiFile(
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 205),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)2 }, 250),
                    new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)2 }, 250),
                    new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)2 }, 350),
                }.ToTrackChunk()));

        #endregion
    }
}
