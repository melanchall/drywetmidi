using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Utility methods for <see cref="TrackChunk"/>.
    /// </summary>
    public static class TrackChunkUtilities
    {
        #region Methods

        /// <summary>
        /// Shifts events forward inside <see cref="TrackChunk"/> by the specified distance.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to shift.</param>
        /// <param name="distance">Distance to shift events by.</param>
        /// <param name="tempoMap">Tempo map used for internal distance conversions.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ShiftEvents(this TrackChunk trackChunk, ITimeSpan distance, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var convertedDistance = TimeConverter.ConvertFrom(distance, TempoMap.Create(tempoMap.TimeDivision));

            var firstEvent = trackChunk.Events.FirstOrDefault();
            if (firstEvent == null)
                return;

            firstEvent.DeltaTime += convertedDistance;
        }

        /// <summary>
        /// Shifts events forward inside collection of <see cref="TrackChunk"/> by the specified distance.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing events to shift.</param>
        /// <param name="distance">Distance to shift events by.</param>
        /// <param name="tempoMap">Tempo map used for internal distance conversions.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ShiftEvents(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.ShiftEvents(distance, tempoMap);
            }
        }

        #endregion
    }
}
