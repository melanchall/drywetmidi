using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiChunkEqualityTests
    {
        #region Nested classes

        private sealed class CustomChunkWithoutEqualsOverride : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunkWithoutEqualsOverride()
                : base(Id)
            {
            }

            public CustomChunkWithoutEqualsOverride(int a)
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
                return new CustomChunkWithoutEqualsOverride(A);
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

            #endregion
        }

        private sealed class CustomChunkWithEqualsOverride : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunkWithEqualsOverride()
                : base(Id)
            {
            }

            public CustomChunkWithEqualsOverride(int a)
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
                return new CustomChunkWithoutEqualsOverride(A);
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

            public override bool Equals(object obj)
            {
                var chunk = obj as CustomChunkWithEqualsOverride;
                if (chunk == null)
                    return false;

                return A == chunk.A;
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public void CompareWithSelf_WithDefaultSettings()
        {
            var trackChunk = new TrackChunk();
            var areEqual = MidiChunk.Equals(trackChunk, trackChunk);
            Assert.IsTrue(areEqual, "Chunk isn't equal to self.");
        }

        [Test]
        public void CompareWithSelf_WithCustomSettings()
        {
            var trackChunk = new TrackChunk();
            var areEqual = MidiChunk.Equals(
                trackChunk,
                trackChunk,
                new MidiChunkEqualityCheckSettings
                {
                    EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                    {
                        CompareDeltaTimes = false
                    }
                },
                out var message);
            Assert.IsTrue(areEqual, "Chunk isn't equal to self.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareWithNull()
        {
            var trackChunk = new TrackChunk();
            var areEqual = MidiChunk.Equals(trackChunk, null, out var message);
            Assert.IsFalse(areEqual, "Chunk is equal to null.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareChunksOfDifferentType()
        {
            var trackChunk = new TrackChunk();
            var customChunk = new CustomChunkWithoutEqualsOverride();

            var areEqual = MidiChunk.Equals(trackChunk, customChunk, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareCustomChunks_WithoutEqualsOverride()
        {
            var customChunk1 = new CustomChunkWithoutEqualsOverride();
            var customChunk2 = new CustomChunkWithoutEqualsOverride();

            var areEqual = MidiChunk.Equals(customChunk1, customChunk2, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareCustomChunks_WithEqualsOverride_Equal()
        {
            var customChunk1 = new CustomChunkWithEqualsOverride();
            var customChunk2 = new CustomChunkWithEqualsOverride();

            var areEqual = MidiChunk.Equals(customChunk1, customChunk2, out var message);
            Assert.IsTrue(areEqual, "Chunks aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareCustomChunks_WithEqualsOverride_Different()
        {
            var customChunk1 = new CustomChunkWithEqualsOverride();
            var customChunk2 = new CustomChunkWithEqualsOverride(8);

            var areEqual = MidiChunk.Equals(customChunk1, customChunk2, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareTrackChunks_DifferentEventsCount()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent());
            var trackChunk2 = new TrackChunk();

            var areEqual = MidiChunk.Equals(trackChunk1, trackChunk2, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareTrackChunks_SameEventsCount_SameEvents()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent());
            var trackChunk2 = new TrackChunk(new NoteOnEvent());

            var areEqual = MidiChunk.Equals(trackChunk1, trackChunk2, out var message);
            Assert.IsTrue(areEqual, "Chunks aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareTrackChunks_SameEventsCount_DifferentEvents()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent());
            var trackChunk2 = new TrackChunk(new NoteOffEvent());

            var areEqual = MidiChunk.Equals(trackChunk1, trackChunk2, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [Test]
        public void CompareTrackChunks_CompareDeltaTimes_SameDeltaTimes()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent { DeltaTime = 100 });
            var trackChunk2 = new TrackChunk(new NoteOnEvent { DeltaTime = 100 });

            var areEqual = MidiChunk.Equals(trackChunk1, trackChunk2, out var message);
            Assert.IsTrue(areEqual, "Chunks aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareTrackChunks_CompareDeltaTimes_DifferentDeltaTimes()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent { DeltaTime = 100 });
            var trackChunk2 = new TrackChunk(new NoteOnEvent { DeltaTime = 1000 });

            var areEqual = MidiChunk.Equals(trackChunk1, trackChunk2, out var message);
            Assert.IsFalse(areEqual, "Chunks are equal.");
            Assert.IsNotNull(message, "Message is null.");
            Assert.IsNotEmpty(message, "Message is empty.");
        }

        [TestCase(100, 100)]
        [TestCase(100, 1000)]
        public void CompareTrackChunks_DontCompareDeltaTimes(long firstDeltaTime, long secondDeltaTime)
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent { DeltaTime = firstDeltaTime });
            var trackChunk2 = new TrackChunk(new NoteOnEvent { DeltaTime = secondDeltaTime });

            var areEqual = MidiChunk.Equals(
                trackChunk1,
                trackChunk2,
                new MidiChunkEqualityCheckSettings
                {
                    EventEqualityCheckSettings = new MidiEventEqualityCheckSettings
                    {
                        CompareDeltaTimes = false
                    }
                },
                out var message);
            Assert.IsTrue(areEqual, "Chunks aren't equal.");
            Assert.IsNull(message, "Message isn't null.");
        }

        #endregion
    }
}
