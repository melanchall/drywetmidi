using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class ChunkTypeTests
    {
        #region Test methods

        [Test]
        public void CheckEquality_SameTypeAndId()
        {
            var chunkType1 = new ChunkType(typeof(TrackChunk), "MTrk");
            var chunkType2 = new ChunkType(typeof(TrackChunk), "MTrk");

            ClassicAssert.IsTrue(chunkType1 == chunkType2, "Chunk types are not equal via ==.");
            ClassicAssert.IsFalse(chunkType1 != chunkType2, "Chunk types are not equal via !=.");
            ClassicAssert.IsTrue(chunkType1.Equals(chunkType2), "Chunk types are not equal via Equals.");
            ClassicAssert.AreEqual(chunkType1.GetHashCode(), chunkType2.GetHashCode(), "Hash codes are different.");
        }

        [Test]
        public void CheckEquality_DifferentType()
        {
            var chunkType1 = new ChunkType(typeof(TrackChunk), "MTrk");
            var chunkType2 = new ChunkType(typeof(HeaderChunk), "MTrk");

            ClassicAssert.IsFalse(chunkType1 == chunkType2, "Chunk types are equal via ==.");
            ClassicAssert.IsTrue(chunkType1 != chunkType2, "Chunk types are equal via !=.");
            ClassicAssert.IsFalse(chunkType1.Equals(chunkType2), "Chunk types are equal via Equals.");
        }

        [Test]
        public void CheckEquality_DifferentId()
        {
            var chunkType1 = new ChunkType(typeof(TrackChunk), "MTrk");
            var chunkType2 = new ChunkType(typeof(TrackChunk), "mtrk");

            ClassicAssert.IsFalse(chunkType1 == chunkType2, "Chunk types are equal via ==.");
            ClassicAssert.IsTrue(chunkType1 != chunkType2, "Chunk types are equal via !=.");
            ClassicAssert.IsFalse(chunkType1.Equals(chunkType2), "Chunk types are equal via Equals.");
        }

        [Test]
        public void CheckEquality_Null()
        {
            var chunkType = new ChunkType(typeof(TrackChunk), "MTrk");
            ChunkType? nullChunkType = null;

            ClassicAssert.IsFalse(chunkType == nullChunkType, "Chunk type is equal to null via ==.");
            ClassicAssert.IsFalse(nullChunkType == chunkType, "Null is equal to chunk type via ==.");
            ClassicAssert.IsTrue(chunkType != nullChunkType, "Chunk type is equal to null via !=.");
            ClassicAssert.IsFalse(chunkType.Equals(null), "Chunk type is equal to null via Equals.");
        }

        #endregion
    }
}
