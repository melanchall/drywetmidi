using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
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

        public static IEnumerable<FourBitNumber> GetChannels(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.OfType<ChannelEvent>().Select(e => e.Channel).Distinct().ToArray();
        }

        public static IEnumerable<FourBitNumber> GetChannels(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.Where(c => c != null).SelectMany(GetChannels).Distinct().ToArray();
        }

        public static void TrimEnd(this TrackChunk trackChunk, Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            var events = trackChunk.Events;

            for (var i = events.Count - 1; i >= 0; i--)
            {
                var midiEvent = events[i];
                if (!match(midiEvent))
                    break;

                events.RemoveAt(i);
            }
        }

        public static void TrimEnd(this IEnumerable<TrackChunk> trackChunks, Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var trackChunksWuthAbsoluteTimes = trackChunks.Select(c =>
            {
                var absoluteTime = 0L;
                var eventsWithTimes = c.Events.Select(e => absoluteTime += e.DeltaTime).ToList();
                return Tuple.Create(c, eventsWithTimes);
            }).ToArray();

            while (true)
            {
                Tuple<TrackChunk, List<long>> trackChunkWithAbsoluteTimes = null;
                long maxTime = -1;

                foreach (var trackChunkWithTimes in trackChunksWuthAbsoluteTimes.Where(c => c.Item1.Events.Any()))
                {
                    var lastTime = trackChunkWithTimes.Item2.LastOrDefault();
                    if (lastTime <= maxTime)
                        continue;

                    trackChunkWithAbsoluteTimes = trackChunkWithTimes;
                    maxTime = lastTime;
                }

                if (trackChunkWithAbsoluteTimes == null)
                    break;

                var events = trackChunkWithAbsoluteTimes.Item1.Events;
                var midiEvent = events.LastOrDefault();
                if (midiEvent == null)
                    break;

                if (!match(midiEvent))
                    break;

                var lastIndex = events.Count - 1;
                events.RemoveAt(lastIndex);
                trackChunkWithAbsoluteTimes.Item2.RemoveAt(lastIndex);
            }
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
