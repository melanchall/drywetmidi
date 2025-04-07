using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MergerTests
    {
        #region Test methods

        [Test]
        public void MergeSimultaneously_NoFiles() => ClassicAssert.Throws<ArgumentException>(() => MergeSimultaneously(
            midiFiles: new MidiFile[0],
            settings: null,
            expectedMidiFile: null));

        [Test]
        public void MergeSimultaneously_EmptyFiles() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(),
                new MidiFile(),
                new MidiFile(),
            },
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void MergeSimultaneously_EmptyFiles_DifferentTpqn_1() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(100) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(50) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) });

        [Test]
        public void MergeSimultaneously_EmptyFiles_DifferentTpqn_2() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(30) });

        [Test]
        public void MergeSimultaneously_EmptyFiles_DifferentTpqn_Failed() => ClassicAssert.Throws<AssertionException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(40) }));

        [Test]
        public void MergeSimultaneously_1() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_2() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("C") { DeltaTime = 40 },
                        new ControlChangeEvent()),
                    new TrackChunk(
                        new TextEvent("D") { DeltaTime = 100 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(80)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 120 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 180 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 80 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C") { DeltaTime = 120 },
                    new ControlChangeEvent()),
                new TrackChunk(
                    new TextEvent("D") { DeltaTime = 300 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(240)
            });

        [Test]
        public void MergeSimultaneously_CustomTempoMaps_1() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_CustomTempoMaps_2() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_1() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(200000),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_2() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new TimeSignatureEvent(5, 8),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_3() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 16) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_4() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 16)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_5() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 10 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_DifferentTempoMaps_6() => ClassicAssert.Throws<InvalidOperationException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300) { DeltaTime = 5 },
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile()));

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_1() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(200000),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(200000),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_2() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new TimeSignatureEvent(5, 8),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new TimeSignatureEvent(5, 8),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_3() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 16) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(5, 16) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_4() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 16)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 16)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_5() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 10 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 40 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_IgnoreDifferentTempoMaps_6() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300),
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(5, 8)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(300) { DeltaTime = 5 },
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(5, 8) { DeltaTime = 5 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                IgnoreDifferentTempoMaps = true
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(300),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent(300) { DeltaTime = 20 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_NonTrackChunk_Copy() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")),
                    new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new CustomChunk(),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSimultaneously_NonTrackChunk_Copy_Fail() => ClassicAssert.Throws<AssertionException>(() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")),
                    new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new CustomChunk(),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3, 4 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            }));

        [Test]
        public void MergeSimultaneously_NonTrackChunk_DontCopy() => MergeSimultaneously(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")),
                    new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SimultaneousMergingSettings
            {
                CopyNonTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        #endregion

        #region Private methods

        private static void MergeSimultaneously(
            ICollection<MidiFile> midiFiles,
            SimultaneousMergingSettings settings,
            MidiFile expectedMidiFile)
        {
            var clonedMidiFiles = midiFiles.Select(f => f.Clone()).ToArray();
            var resultMidiFile = midiFiles.MergeSimultaneously(settings);

            MidiAsserts.AreEqual(expectedMidiFile, resultMidiFile, false, "Invalid file after processing.");
            MidiAsserts.AreEqual(clonedMidiFiles, midiFiles, false, "Original files are modified after processing.");

            CheckEventsAreCloned(clonedMidiFiles, resultMidiFile);
        }

        #endregion
    }
}
