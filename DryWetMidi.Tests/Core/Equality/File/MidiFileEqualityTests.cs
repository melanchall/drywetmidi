using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiFileEqualityTests
    {
        #region Test methods

        [Test]
        public void CompareOriginalFormat_Created()
        {
            var midiFile1 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));
            var midiFile2 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = true },
                out var message);
            Assert.IsTrue(areEqual, "Files aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareOriginalFormat_Read_SameFormats()
        {
            var midiFile1 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                MidiFileFormat.MultiTrack);
            var midiFile2 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                MidiFileFormat.MultiTrack);

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = true },
                out var message);
            Assert.IsTrue(areEqual, "Files aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareOriginalFormat_Read_DifferentFormats()
        {
            var midiFile1 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                MidiFileFormat.MultiTrack);
            var midiFile2 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                MidiFileFormat.SingleTrack);

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = true },
                out var message);
            Assert.IsFalse(areEqual, "Files are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void DontCompareOriginalFormat_Created()
        {
            var midiFile1 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));
            var midiFile2 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = false },
                out var message);
            Assert.IsTrue(areEqual, "Files aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [TestCase(MidiFileFormat.MultiTrack, MidiFileFormat.MultiTrack)]
        [TestCase(MidiFileFormat.MultiTrack, MidiFileFormat.SingleTrack)]
        public void DontCompareOriginalFormat_Read(MidiFileFormat firstFormat, MidiFileFormat secondFormat)
        {
            var midiFile1 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                firstFormat);
            var midiFile2 = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 })),
                null,
                null,
                secondFormat);

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings { CompareOriginalFormat = false },
                out var message);
            Assert.IsTrue(areEqual, "Files aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareDeltaTimes_SameDeltaTimes()
        {
            var midiFile1 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));
            var midiFile2 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));

            var areEqual = MidiFile.Equals(midiFile1, midiFile2, out var message);
            Assert.IsTrue(areEqual, "Files aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareDeltaTimes_DifferentDeltaTimes()
        {
            var midiFile1 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 100 }));
            var midiFile2 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = 1000 }));

            var areEqual = MidiFile.Equals(midiFile1, midiFile2, out var message);
            Assert.IsFalse(areEqual, "Files are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [TestCase(100, 100)]
        [TestCase(100, 1000)]
        public void DontCompareDeltaTimes(long firstDeltaTime, long secondDeltaTime)
        {
            var midiFile1 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = firstDeltaTime }));
            var midiFile2 = new MidiFile(new TrackChunk(new NoteOnEvent { DeltaTime = secondDeltaTime }));

            var areEqual = MidiFile.Equals(
                midiFile1,
                midiFile2,
                new MidiFileEqualityCheckSettings
                {
                    ChunkEqualityCheckSettings = new MidiChunkEqualityCheckSettings
                    {
                        EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                        {
                            CompareDeltaTimes = false
                        }
                    }
                },
                out var message);
            Assert.IsTrue(areEqual, "Chunks aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        #endregion
    }
}
