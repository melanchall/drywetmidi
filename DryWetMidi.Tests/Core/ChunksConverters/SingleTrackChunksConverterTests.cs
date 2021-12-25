using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class SingleTrackChunksConverterTests
    {
        #region Test methods

        [Test]
        public void Convert_EmptyCollection() => Convert(
            chunks: Enumerable.Empty<MidiChunk>(),
            expectedChunks: Enumerable.Empty<MidiChunk>());

        [Test]
        public void Convert_SingleChunk_TrackChunk() => Convert(
            chunks: new[] { new TrackChunk() },
            expectedChunks: new[] { new TrackChunk() });

        [Test]
        public void Convert_SingleChunk_UnknownChunk() => Convert(
            chunks: new[] { new UnknownChunk("abcd") },
            expectedChunks: new[] { new UnknownChunk("abcd") });

        #endregion

        #region Private methods

        private void Convert(
            IEnumerable<MidiChunk> chunks,
            IEnumerable<MidiChunk> expectedChunks)
        {
            var converter = new SingleTrackChunksConverter();
            var actualChunks = converter.Convert(chunks);
            MidiAsserts.AreEqual(expectedChunks, actualChunks, true, "Invalid resulting chunks.");
        }

        #endregion
    }
}
