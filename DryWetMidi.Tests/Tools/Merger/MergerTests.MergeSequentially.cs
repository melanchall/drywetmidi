using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
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
        #region Nested classes

        private sealed class CustomChunk : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunk()
                : base(Id)
            {
            }

            public CustomChunk(int a)
                : this()
            {
                A = a;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new CustomChunk(A);
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return (uint)DataTypesUtilities.GetVlqLength(A);
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                A = reader.ReadVlqNumber();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
            }

            public override bool Equals(object obj) =>
                obj is CustomChunk customChunk &&
                customChunk.A == A;

            public override int GetHashCode() =>
                A.GetHashCode();

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public void MergeSequentially_NoFiles() => ClassicAssert.Throws<ArgumentException>(() => MergeSequentially(
            midiFiles: new MidiFile[0],
            settings: null,
            expectedMidiFile: null));

        [Test]
        public void MergeSequentially_EmptyFiles() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(),
                new MidiFile(),
                new MidiFile(),
            },
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void MergeSequentially_EmptyFiles_DifferentTpqn_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(100) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(50) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) });

        [Test]
        public void MergeSequentially_EmptyFiles_DifferentTpqn_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(30) });

        [Test]
        public void MergeSequentially_EmptyFiles_DifferentTpqn_Failed() => ClassicAssert.Throws<AssertionException>(() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(40) }));

        [Test]
        public void MergeSequentially_1() => MergeSequentially(
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
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_2() => MergeSequentially(
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
                    new ControlChangeEvent() { DeltaTime = 260 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C") { DeltaTime = 380 },
                    new ControlChangeEvent()),
                new TrackChunk(
                    new TextEvent("D") { DeltaTime = 560 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(240)
            });

        [Test]
        public void MergeSequentially_SetTempo_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new SetTempoEvent(200)),
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
                    new PitchBendEvent() { DeltaTime = 60 },
                    new SetTempoEvent(200)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_SetTempo_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new SetTempoEvent(200)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(),
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
                    new PitchBendEvent() { DeltaTime = 60 },
                    new SetTempoEvent(200)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_SetTempo_3() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new SetTempoEvent(200),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new SetTempoEvent(500)))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new SetTempoEvent(500)))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_SetTempo_4() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new SetTempoEvent(),
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
                    new SetTempoEvent(),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_SetTempo_5() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 24 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 32 })),
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 48 },
                        new SetTempoEvent(200),
                        new TextEvent("C"))),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("D") { DeltaTime = 24 })),
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 24 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 32 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 80 },
                    new SetTempoEvent(200),
                    new TextEvent("C")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 80 },
                    new TextEvent("D") { DeltaTime = 24 })));

        [Test]
        public void MergeSequentially_PitchBend_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent(200) { DeltaTime = 20 }),
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
                    new PitchBendEvent(200) { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_PitchBend_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent(200) { DeltaTime = 20, Channel = (FourBitNumber)5 }),
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
                    new PitchBendEvent(200) { DeltaTime = 60, Channel = (FourBitNumber)5 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 90, Channel = (FourBitNumber)5 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_PitchBend_3() => MergeSequentially(
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
                        new PitchBendEvent(),
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
                    new PitchBendEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_PitchBend_4() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20, Channel = (FourBitNumber)5 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(),
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
                    new PitchBendEvent() { DeltaTime = 60, Channel = (FourBitNumber)5 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_PitchBend_5() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new SetTempoEvent(),
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
                    new SetTempoEvent(),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_PitchBend_6() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 24 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 32 })),
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 48 },
                        new PitchBendEvent(200),
                        new TextEvent("C"))),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("D") { DeltaTime = 24 },
                        new PitchBendEvent(500) { Channel = (FourBitNumber)5 })),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("E") { DeltaTime = 24 })),
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 24 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 32 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 80 },
                    new PitchBendEvent(200),
                    new TextEvent("C")),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 80 },
                    new TextEvent("D") { DeltaTime = 24 },
                    new PitchBendEvent(500) { Channel = (FourBitNumber)5 }),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 104, Channel = (FourBitNumber)5 },
                    new PitchBendEvent(),
                    new TextEvent("E") { DeltaTime = 24 })));

        [Test]
        public void MergeSequentially_TimeSignature_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
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
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_TimeSignature_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick + 1,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new TimeSignatureEvent(),
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
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick + 1,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_TimeSignature_3() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat + 1),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator + 1,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat + 1),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator + 1,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_DelayBetweenFiles_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 280 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_DelayBetweenFiles_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 120 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 180 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 560 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C") { DeltaTime = 1080 },
                    new ControlChangeEvent()),
                new TrackChunk(
                    new TextEvent("D") { DeltaTime = 1260 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(240)
            });

        [Test]
        public void MergeSequentially_DelayBetweenFiles_3() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = new MetricTimeSpan(0, 0, 1)
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 370 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_NonTrackChunk_Copy() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
                new CustomChunk(30),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_NonTrackChunk_Copy_Fail() => ClassicAssert.Throws<AssertionException>(() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
                new CustomChunk(30),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3, 4 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            }));

        [Test]
        public void MergeSequentially_NonTrackChunk_DontCopy() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
            settings: new SequentialMergingSettings
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
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileDurationRoundingStep_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50,
                FileDurationRoundingStep = (MidiTimeSpan)20
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 310 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileDurationRoundingStep_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = new MetricTimeSpan(0, 0, 5)
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 1240 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileStartMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileStartMarkerEventFactory = file => new MarkerEvent("Start")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new MarkerEvent("Start") { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileStartMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileStartMarkerEventFactory = file => new MarkerEvent("Start")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 }),
                new TrackChunk(
                    new MarkerEvent("Start") { DeltaTime = 210 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileEndMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileEndMarkerEventFactory = file => new MarkerEvent("End")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End")),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B"),
                    new MarkerEvent("End")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileEndMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileEndMarkerEventFactory = file => new MarkerEvent("End")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End") { DeltaTime = 30 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 250 },
                    new TextEvent("B"),
                    new MarkerEvent("End") { DeltaTime = 40 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileEndMarkerEventFactory_FileEndMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                FileEndMarkerEventFactory = file => new MarkerEvent("End")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 30 }),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End")),
                new TrackChunk(
                    new MarkerEvent("Start") { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new MarkerEvent("End")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_FileEndMarkerEventFactory_FileEndMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                FileEndMarkerEventFactory = file => new MarkerEvent(file.Chunks.Count > 1 ? "End1" : "End2")
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End1") { DeltaTime = 60 }),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End1") { DeltaTime = 30 }),
                new TrackChunk(
                    new MarkerEvent("Start") { DeltaTime = 210 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new MarkerEvent("End2") { DeltaTime = 40 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_NoFiles() => ClassicAssert.Throws<ArgumentException>(() => MergeSequentially(
            midiFiles: new MidiFile[0],
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: null));

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_EmptyFiles() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(),
                new MidiFile(),
                new MidiFile(),
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile());

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_EmptyFiles_DifferentTpqn_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(100) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(50) },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(200) });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_EmptyFiles_DifferentTpqn_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(30) });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_EmptyFiles_DifferentTpqn_Failed() => ClassicAssert.Throws<AssertionException>(() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(2) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(5) },
                new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(15) },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(40) }));

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 70 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 120 },
                    new ControlChangeEvent() { DeltaTime = 140 },
                    new TextEvent("B"),
                    new TextEvent("C") { DeltaTime = 120 },
                    new ControlChangeEvent()),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 180 },
                    new TextEvent("D") { DeltaTime = 380 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(240)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_3() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 130 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_SetTempo_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new SetTempoEvent(200)),
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
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new SetTempoEvent(200),
                    new SetTempoEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_SetTempo_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new SetTempoEvent(200)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new SetTempoEvent(200),
                    new SetTempoEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_SetTempo_3() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new SetTempoEvent(200),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new SetTempoEvent(500)))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new SetTempoEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new SetTempoEvent(500)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_TimeSignature_1() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
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
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(5, 8),
                    new TimeSignatureEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_TimeSignature_2() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 },
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick + 1,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new TimeSignatureEvent(),
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick + 1,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat),
                    new TimeSignatureEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_TimeSignature_3() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat + 1),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 30 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(40)
                },
                new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent() { DeltaTime = 10 },
                        new TextEvent("B"),
                        new TimeSignatureEvent(
                            TimeSignatureEvent.DefaultNumerator + 1,
                            TimeSignatureEvent.DefaultDenominator,
                            TimeSignatureEvent.DefaultClocksPerClick,
                            TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(30)
                },
            },
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat + 1),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new TimeSignatureEvent() { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(
                        TimeSignatureEvent.DefaultNumerator + 1,
                        TimeSignatureEvent.DefaultDenominator,
                        TimeSignatureEvent.DefaultClocksPerClick,
                        TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_DelayBetweenFiles_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50,
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 220 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_DelayBetweenFiles_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50,
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 120 },
                    new ControlChangeEvent() { DeltaTime = 440 },
                    new TextEvent("B"),
                    new TextEvent("C") { DeltaTime = 520 },
                    new ControlChangeEvent()),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 180 },
                    new TextEvent("D") { DeltaTime = 1080 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(240)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_DelayBetweenFiles_3() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = new MetricTimeSpan(0, 0, 1),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 310 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_NonTrackChunk_Copy() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 70 },
                    new TextEvent("B")),
                new CustomChunk(30),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_NonTrackChunk_Copy_Fail() => ClassicAssert.Throws<AssertionException>(() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
            settings: new SequentialMergingSettings
            {
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 70 },
                    new TextEvent("B")),
                new CustomChunk(30),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3, 4 } })
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            }));

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_NonTrackChunk_DontCopy() => MergeSequentially(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new PitchBendEvent() { DeltaTime = 20 }),
                    new CustomChunk(30),
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
            settings: new SequentialMergingSettings
            {
                CopyNonTrackChunks = false,
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 70 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileDurationRoundingStep_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                DelayBetweenFiles = (MidiTimeSpan)50,
                FileDurationRoundingStep = (MidiTimeSpan)20,
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 250 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileDurationRoundingStep_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = new MetricTimeSpan(0, 0, 5),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 1180 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileStartMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("Start") { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileStartMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("Start") { DeltaTime = 150 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B")),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileEndMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileEndMarkerEventFactory = file => new MarkerEvent("End"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 30 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new MarkerEvent("End")),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileEndMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileEndMarkerEventFactory = file => new MarkerEvent("End"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 60 },
                    new ControlChangeEvent() { DeltaTime = 130 },
                    new TextEvent("B"),
                    new MarkerEvent("End") { DeltaTime = 40 }),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End") { DeltaTime = 30 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileEndMarkerEventFactory_FileEndMarkerEventFactory_1() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                FileEndMarkerEventFactory = file => new MarkerEvent("End"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End") { DeltaTime = 30 },
                    new MarkerEvent("Start"),
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new MarkerEvent("End")),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End")))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        [Test]
        public void MergeSequentially_MinimizeTrackChunksCount_FileEndMarkerEventFactory_FileEndMarkerEventFactory_2() => MergeSequentially(
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
            settings: new SequentialMergingSettings
            {
                FileDurationRoundingStep = (MidiTimeSpan)20,
                DelayBetweenFiles = (MidiTimeSpan)30,
                FileStartMarkerEventFactory = file => new MarkerEvent("Start"),
                FileEndMarkerEventFactory = file => new MarkerEvent(file.Chunks.Count > 1 ? "End1" : "End2"),
                ResultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.MinimizeCount
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 60 },
                    new MarkerEvent("End1") { DeltaTime = 60 },
                    new MarkerEvent("Start") { DeltaTime = 90 },
                    new ControlChangeEvent() { DeltaTime = 40 },
                    new TextEvent("B"),
                    new MarkerEvent("End2") { DeltaTime = 40 }),
                new TrackChunk(
                    new MarkerEvent("Start"),
                    new TextEvent("B") { DeltaTime = 90 },
                    new MarkerEvent("End1") { DeltaTime = 30 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(120)
            });

        #endregion

        #region Private methods

        private static void MergeSequentially(
            ICollection<MidiFile> midiFiles,
            SequentialMergingSettings settings,
            MidiFile expectedMidiFile)
        {
            var clonedMidiFiles = midiFiles.Select(f => f.Clone()).ToArray();
            var resultMidiFile = midiFiles.MergeSequentially(settings);
            
            MidiAsserts.AreEqual(expectedMidiFile, resultMidiFile, false, "Invalid file after processing.");
            MidiAsserts.AreEqual(clonedMidiFiles, midiFiles, false, "Original files are modified after processing.");

            CheckEventsAreCloned(clonedMidiFiles, resultMidiFile);

            CheckDurationAsSum(clonedMidiFiles, resultMidiFile, TimeSpanType.Musical, settings);
            CheckDurationAsSum(clonedMidiFiles, resultMidiFile, TimeSpanType.Metric, settings);
        }

        private static void CheckEventsAreCloned(
            MidiFile[] originalFiles,
            MidiFile resultFile)
        {
            MidiEvent[] GetEvents(IEnumerable<MidiFile> files) =>
                files.SelectMany(f => (f?.GetTrackChunks() ?? Enumerable.Empty<TrackChunk>()).SelectMany(c => c.Events)).ToArray();

            var originalEvents = GetEvents(originalFiles);
            var newEvents = GetEvents(new[] { resultFile });
            ClassicAssert.IsFalse(newEvents.Any(e => originalEvents.Contains(e)), "Result file contains original events.");
        }

        private static void CheckDurationAsSum(
            MidiFile[] originalFiles,
            MidiFile resultFile,
            TimeSpanType timeSpanType,
            SequentialMergingSettings settings)
        {
            var expectedLength = TimeSpanUtilities.GetZeroTimeSpan(timeSpanType);
            var delay = settings?.DelayBetweenFiles;
            var roundingStep = settings?.FileDurationRoundingStep;

            for (var i = 0; i < originalFiles.Length; i++)
            {
                var midiFile = originalFiles[i];
                var tempoMap = midiFile.GetTempoMap();

                var duration = midiFile.GetDuration(timeSpanType);
                if ((settings?.FileEndMarkerEventFactory != null || i < originalFiles.Length - 1) && roundingStep != null)
                {
                    var convertedStep = TimeConverter.ConvertTo(roundingStep, timeSpanType, tempoMap);
                    duration = duration.Round(TimeSpanRoundingPolicy.RoundUp, (MidiTimeSpan)0, convertedStep, tempoMap);
                }

                expectedLength = expectedLength.Add(duration, TimeSpanMode.LengthLength);

                if (i < originalFiles.Length - 1 && delay != null)
                {
                    var convertedDelay = LengthConverter.ConvertTo(delay, timeSpanType, duration, midiFile.GetTempoMap());
                    expectedLength = expectedLength.Add(convertedDelay, TimeSpanMode.LengthLength);
                }
            }

            var actualLength = resultFile.GetDuration(timeSpanType);
            ClassicAssert.AreEqual(expectedLength, actualLength, $"Invalid length of {timeSpanType} type.");
        }

        #endregion
    }
}
