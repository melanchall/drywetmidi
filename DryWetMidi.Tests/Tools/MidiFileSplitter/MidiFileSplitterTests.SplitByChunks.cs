using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
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
                throw new System.NotImplementedException();
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                throw new System.NotImplementedException();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                throw new System.NotImplementedException();
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
            expectedFilesCount: 0);

        [Test]
        public void SplitByChunks_OneChunk_TrackChunk() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFilesCount: 1);

        [Test]
        public void SplitByChunks_OneChunk_TrackChunk_TimeDivision() => SplitByChunks(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedFilesCount: 1,
            timeDivision: new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100));

        [Test]
        public void SplitByChunks_OneChunk_CustomChunk() => SplitByChunks(
            chunks: new[] { new CustomChunk() },
            expectedFilesCount: 1);

        [Test]
        public void SplitByChunks_MultipleChunks_1() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A")),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B")),
                new TrackChunk(new TextEvent("C")),
            },
            expectedFilesCount: 4);

        [Test]
        public void SplitByChunks_MultipleChunks_2() => SplitByChunks(
            chunks: new MidiChunk[]
            {
                new CustomChunk(),
                new UnknownChunk("1111"),
                new UnknownChunk("2222"),
            },
            expectedFilesCount: 3);

        [Test]
        public void SplitByChunks_Filter_NoChunks() => SplitByChunks_WithSettings(
            chunks: new MidiChunk[0],
            expectedChunks: new MidiChunk[0],
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_1() => SplitByChunks_WithSettings(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedChunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_2() => SplitByChunks_WithSettings(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedChunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_3() => SplitByChunks_WithSettings(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedChunks: new MidiChunk[0],
            settings: new SplitFileByChunksSettings { Filter = c => !(c is TrackChunk) });

        [Test]
        public void SplitByChunks_Filter_OneChunk_TrackChunk_TimeDivision() => SplitByChunks_WithSettings(
            chunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            expectedChunks: new[] { new TrackChunk(new NoteOnEvent(), new NoteOffEvent()) },
            settings: new SplitFileByChunksSettings { Filter = c => true },
            timeDivision: new SmpteTimeDivision(DryWetMidi.Common.SmpteFormat.ThirtyDrop, 100));

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_1() => SplitByChunks_WithSettings(
            chunks: new[] { new CustomChunk() },
            expectedChunks: new[] { new CustomChunk() },
            settings: new SplitFileByChunksSettings { Filter = null });

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_2() => SplitByChunks_WithSettings(
            chunks: new[] { new CustomChunk() },
            expectedChunks: new MidiChunk[0],
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_OneChunk_CustomChunk_3() => SplitByChunks_WithSettings(
            chunks: new[] { new CustomChunk() },
            expectedChunks: new[] { new CustomChunk() },
            settings: new SplitFileByChunksSettings { Filter = c => c is CustomChunk });

        [Test]
        public void SplitByChunks_Filter_MultipleChunks_1() => SplitByChunks_WithSettings(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A")),
                new CustomChunk(),
                new TrackChunk(new TextEvent("B")),
                new TrackChunk(new TextEvent("C")),
            },
            expectedChunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new TextEvent("B")),
                new TrackChunk(new TextEvent("C")),
            },
            settings: new SplitFileByChunksSettings { Filter = c => c is TrackChunk });

        [Test]
        public void SplitByChunks_Filter_MultipleChunks_2() => SplitByChunks_WithSettings(
            chunks: new MidiChunk[]
            {
                new CustomChunk(),
                new UnknownChunk("1111"),
                new UnknownChunk("2222"),
            },
            expectedChunks: new MidiChunk[]
            {
                new UnknownChunk("1111"),
                new UnknownChunk("2222"),
            },
            settings: new SplitFileByChunksSettings { Filter = c => !(c is CustomChunk) });

        #endregion

        #region Private methods

        private void SplitByChunks(
            ICollection<MidiChunk> chunks,
            int expectedFilesCount,
            TimeDivision timeDivision = null)
        {
            var midiFile = new MidiFile(chunks);
            if (timeDivision != null)
                midiFile.TimeDivision = timeDivision;

            var midiFilesByChunks = midiFile.SplitByChunks().ToList();

            Assert.AreEqual(expectedFilesCount, midiFilesByChunks.Count, "Invalid count of new files.");

            var i = 0;

            foreach (var chunk in chunks)
            {
                MidiAsserts.AreEqual(
                    new MidiFile(chunk) { TimeDivision = midiFile.TimeDivision },
                    midiFilesByChunks[i],
                    false,
                    $"File {i} is invalid.");

                i++;
            }
        }

        private void SplitByChunks_WithSettings(
            ICollection<MidiChunk> chunks,
            ICollection<MidiChunk> expectedChunks,
            SplitFileByChunksSettings settings,
            TimeDivision timeDivision = null)
        {
            var midiFile = new MidiFile(chunks);
            if (timeDivision != null)
                midiFile.TimeDivision = timeDivision;

            var midiFilesByChunks = midiFile.SplitByChunks(settings).ToList();

            Assert.AreEqual(expectedChunks.Count, midiFilesByChunks.Count, "Invalid count of new files.");

            var i = 0;

            foreach (var chunk in expectedChunks)
            {
                MidiAsserts.AreEqual(
                    new MidiFile(chunk) { TimeDivision = midiFile.TimeDivision },
                    midiFilesByChunks[i],
                    false,
                    $"File {i} is invalid.");

                i++;
            }
        }

        #endregion
    }
}
