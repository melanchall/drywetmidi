using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
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
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<TrackChunk> GetTrackChunks(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.Chunks.OfType<TrackChunk>();
        }

        /// <summary>
        /// Merges multiple track chunks into one that corresponds to <see cref="MidiFileFormat.SingleTrack"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to merge into one.</param>
        /// <returns>Track chunk that contains all events from the <paramref name="trackChunks"/>.</returns>
        /// <remarks>
        /// Note that events will be cloned so events in the result track chunk will not be equal
        /// by reference to events in the <paramref name="trackChunks"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="trackChunks"/> is an empty collection.</exception>
        public static TrackChunk Merge(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsEmptyCollection(nameof(trackChunks), trackChunks, "Track chunks collection is empty.");

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
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static IEnumerable<TrackChunk> Explode(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return ConvertTrackChunks(new[] { trackChunk }, MidiFileFormat.MultiTrack);
        }

        /// <summary>
        /// Gets all channel numbers presented in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to get channels of.</param>
        /// <returns>Collection of channel numbers presented in the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static IEnumerable<FourBitNumber> GetChannels(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.OfType<ChannelEvent>().Select(e => e.Channel).Distinct().ToArray();
        }

        /// <summary>
        /// Gets all channel numbers presented in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to get channels of.</param>
        /// <returns>Collection of channel numbers presented in the <paramref name="trackChunks"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static IEnumerable<FourBitNumber> GetChannels(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.Where(c => c != null).SelectMany(GetChannels).Distinct().ToArray();
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
