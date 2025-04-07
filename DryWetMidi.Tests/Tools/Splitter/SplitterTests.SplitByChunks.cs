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
    public sealed partial class SplitterTests
    {
        #region Nested classes

        private sealed class CustomChunk : MidiChunk
        {
            public CustomChunk()
                : base("Cstm")
            {
            }

            public override MidiChunk Clone()
            {
                return new CustomChunk();
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                throw new NotImplementedException();
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                throw new NotImplementedException();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                return obj is CustomChunk;
            }

            public override int GetHashCode()
            {
                return ChunkId.GetHashCode();
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void SplitByChunks_NoChunks() => SplitByChunks(
            chunks: new MidiChunk[0],
            expectedFiles: new MidiFile[0]);

        [Test]
        public void SplitByChunks_OneChunk_TrackChunk() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new NoteOnEvent(), new NoteOffEvent()))
            });

        [Test]
        public void SplitByChunks_OneChunk_TrackChunk_TempoMap() => SplitByChunks(
            chunks: new[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new SetTempoEvent(100000) { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                    new TimeSignatureEvent(2, 4))
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new SetTempoEvent(100000) { DeltaTime = 10 },
                        new NoteOffEvent { DeltaTime = 100 },
                        new TimeSignatureEvent(2, 4)))
            });

        [Test]
        public void SplitByChunks_OneChunk_TrackChunk_TimeDivision() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new NoteOnEvent(), new NoteOffEvent()))
                {
                    TimeDivision = new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100)
                }
            },
            timeDivision: new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100));

        [Test]
        public void SplitByChunks_OneChunk_CustomChunk() => SplitByChunks(
            chunks: new[] { new CustomChunk() },
            expectedFiles: new[]
            {
                new MidiFile(
                    new CustomChunk())
            });

        [Test]
        public void SplitByChunks_MultipleChunks_1() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A")),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B")),
                new TrackChunk(new TextEvent("C")),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new TextEvent("A"))),
                new MidiFile(
                    new CustomChunk()),
                new MidiFile(
                    new TrackChunk(new TextEvent("B"))),
                new MidiFile(
                    new TrackChunk(new TextEvent("C"))),
            });

        [Test]
        public void SplitByChunks_MultipleChunks_2() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new CustomChunk(),
                new UnknownChunk("1111"),
                new UnknownChunk("2222"),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new CustomChunk()),
                new MidiFile(
                    new UnknownChunk("1111")),
                new MidiFile(
                    new UnknownChunk("2222")),
            });

        [Test]
        public void SplitByChunks_MultipleChunks_3() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new TrackChunk(
                    new SetTempoEvent(100000),
                    new TextEvent("A") { DeltaTime = 10 },
                    new TimeSignatureEvent(2, 4) { DeltaTime = 100 }),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B") { DeltaTime = 10 }),
                new TrackChunk(new TextEvent("C") { DeltaTime = 10 }),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(100000),
                        new TextEvent("A") { DeltaTime = 10 },
                        new TimeSignatureEvent(2, 4) { DeltaTime = 100 })),
                new MidiFile(
                    new CustomChunk(),
                    new TrackChunk(
                        new SetTempoEvent(100000),
                        new TimeSignatureEvent(2, 4) { DeltaTime = 110 })),
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(100000),
                        new TextEvent("B") { DeltaTime = 10 },
                        new TimeSignatureEvent(2, 4) { DeltaTime = 100 })),
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(100000),
                        new TextEvent("C") { DeltaTime = 10 },
                        new TimeSignatureEvent(2, 4) { DeltaTime = 100 })),
            });

        [Test]
        public void SplitByChunks_Filter_NoChunks() => SplitByChunks(
            chunks: new MidiChunk[0],
            expectedFiles: new MidiFile[0],
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_MultipleChunks_DontPreserveTempoMap() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new TrackChunk(
                    new SetTempoEvent(100000),
                    new TextEvent("A") { DeltaTime = 10 },
                    new TimeSignatureEvent(2, 4) { DeltaTime = 100 }),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B") { DeltaTime = 10 }),
                new TrackChunk(new TextEvent("C") { DeltaTime = 10 }),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(100000),
                        new TextEvent("A") { DeltaTime = 10 },
                        new TimeSignatureEvent(2, 4) { DeltaTime = 100 })),
                new MidiFile(
                    new CustomChunk()),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = 10 })),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("C") { DeltaTime = 10 })),
            },
            settings: new SplitFileByChunksSettings
            {
                PreserveTempoMap = false
            });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_1() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new NoteOnEvent(), new NoteOffEvent()))
            },
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_2() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new NoteOnEvent(), new NoteOffEvent()))
            },
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_3() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new MidiFile[0],
            settings: new SplitFileByChunksSettings { Filter = c => !(c is TrackChunk) });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_TimeDivision() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new NoteOnEvent(), new NoteOffEvent()))
                {
                    TimeDivision = new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100)
                }
            },
            settings: new SplitFileByChunksSettings { Filter = c => true },
            timeDivision: new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100));

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_1() => SplitByChunks(
            chunks: new[] { new CustomChunk() },
            expectedFiles: new[]
            {
                new MidiFile(
                    new CustomChunk())
            },
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_2() => SplitByChunks(
            chunks: new[] { new CustomChunk() },
            expectedFiles: new MidiFile[0],
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_3() => SplitByChunks(
            chunks: new[] { new CustomChunk() },
            expectedFiles: new[]
            {
                new MidiFile(
                    new CustomChunk())
            },
            settings: new SplitFileByChunksSettings { Filter = c => c is CustomChunk });

        [Test]
        public void SplitByChunks_Filter_MultipleChunks_1() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A")),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B")),
                new TrackChunk(new TextEvent("C")),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(new TextEvent("A"))),
                new MidiFile(
                    new TrackChunk(new TextEvent("B"))),
                new MidiFile(
                    new TrackChunk(new TextEvent("C")))
            },
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_MultipleChunks_2() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new CustomChunk(),
                new UnknownChunk("1111"),
                new UnknownChunk("2222"),
            },
            expectedFiles: new[]
            {
                new MidiFile(
                    new UnknownChunk("1111")),
                new MidiFile(
                    new UnknownChunk("2222"))
            },
            settings: new SplitFileByChunksSettings { Filter = c => !(c is CustomChunk) });

        #endregion

        #region Private methods

        private void SplitByChunks(
            ICollection<MidiChunk> chunks,
            ICollection<MidiFile> expectedFiles,
            SplitFileByChunksSettings settings = null,
            TimeDivision timeDivision = null)
        {
            var midiFile = new MidiFile(chunks);
            if (timeDivision != null)
                midiFile.TimeDivision = timeDivision;

            var midiFilesByChunks = midiFile.SplitByChunks(settings).ToList();

            ClassicAssert.AreEqual(expectedFiles.Count, midiFilesByChunks.Count, "Invalid count of new files.");

            var i = 0;

            foreach (var file in expectedFiles)
            {
                MidiAsserts.AreEqual(
                    file,
                    midiFilesByChunks[i],
                    false,
                    $"File {i} is invalid.");

                i++;
            }
        }

        #endregion
    }
}
