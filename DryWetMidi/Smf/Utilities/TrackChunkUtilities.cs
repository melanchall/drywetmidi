using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provides extension methods for <see cref="TrackChunk"/>.
    /// </summary>
    public static class TrackChunkUtilities
    {
        #region Methods

        /// <summary>
        /// Gets all track chunks of a MIDI file.
        /// </summary>
        /// <param name="midiFile">MIDI file to get track chunks of.</param>
        /// <returns>Collection of track chunks contained in the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        public static IEnumerable<TrackChunk> GetTrackChunks(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.Chunks.OfType<TrackChunk>();
        }

        /// <summary>
        /// Merges multiple track chunks into one that corresponds to <see cref="MidiFileFormat.SingleTrack"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to merge into one.</param>
        /// <returns>Track chunk that containes all events from the <paramref name="trackChunks"/>.</returns>
        /// <remarks>
        /// Note that events will be cloned so events in the result track chunk will not be equal
        /// by reference to events in the <paramref name="trackChunks"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null.</exception>
        public static TrackChunk Merge(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return ConvertTrackChunks(trackChunks, MidiFileFormat.SingleTrack).First();

        }

        /// <summary>
        /// Splits a track chunk into multiple ones that correspond to <see cref="MidiFileFormat.MultiTrack"/>.
        /// </summary>
        /// <param name="trackChunk">Track chunk to split into multiple ones.</param>
        /// <returns>Multiple track chunks that represent <paramref name="trackChunk"/>.</returns>
        /// <remarks>
        /// Note that events will be cloned so events in the result track chunks will not be equal
        /// by reference to events in the <paramref name="trackChunk"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null.</exception>
        public static IEnumerable<TrackChunk> Explode(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return ConvertTrackChunks(new[] { trackChunk }, MidiFileFormat.MultiTrack);

        }

        private static IEnumerable<TrackChunk> ConvertTrackChunks(IEnumerable<TrackChunk> trackChunks, MidiFileFormat format)
        {
            var chunksConverter = ChunksConverterFactory.GetConverter(format);
            return chunksConverter.Convert(trackChunks)
                                  .OfType<TrackChunk>();
        } 
    
        #endregion
    }
}
