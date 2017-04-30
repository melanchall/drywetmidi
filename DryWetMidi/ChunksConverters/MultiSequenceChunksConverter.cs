using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryMidi
{
    internal sealed class MultiSequenceChunksConverter : IChunksConverter
    {
        #region IChunksConverter

        public IEnumerable<Chunk> Convert(IEnumerable<Chunk> chunks)
        {
            if (chunks == null)
                throw new ArgumentNullException(nameof(chunks));

            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length == 0)
                return chunks;

            var sequenceNumbers = trackChunks.Select(GetSequenceNumber).ToArray();
            if (!sequenceNumbers.Any() || sequenceNumbers.Any(n => n == null))
                throw new InvalidOperationException("Chunks cannot be converted to multi-sequence since some of chunks have no sequence number.");

            if (sequenceNumbers.Length == trackChunks.Length)
                return chunks;

            var singleTrackChunksConverter = ChunksConverterFactory.GetConverter(MidiFileFormat.SingleTrack);
            return trackChunks.GroupBy(GetSequenceNumber)
                              .SelectMany(g => singleTrackChunksConverter.Convert(g))
                              .Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion

        #region Methods

        private static short? GetSequenceNumber(TrackChunk trackChunk)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            return trackChunk.Events
                             .TakeWhile(m => m.DeltaTime == 0)
                             .OfType<SequenceNumberEvent>()
                             .FirstOrDefault()
                             ?.Number;
        }

        #endregion
    }
}
