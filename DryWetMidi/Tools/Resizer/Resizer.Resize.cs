using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to resize MIDI data in many different ways. More info in the
    /// <see href="xref:a_resizer">Resizer</see> article.
    /// </summary>
    public static partial class Resizer
    {
        #region Methods

        /// <summary>
        /// Resizes <see cref="TrackChunk"/> to the specified length. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to resize.</param>
        /// <param name="length">New length of the <paramref name="trackChunk"/>.</param>
        /// <param name="tempoMap">Tempo map used to calculate length.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Resize(this TrackChunk trackChunk, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!trackChunk.Events.Any())
                return;

            var duration = trackChunk.GetDuration<MidiTimeSpan>(tempoMap);
            if (duration.IsZeroTimeSpan())
                return;

            var ratio = GetRatio(duration, length, tempoMap);

            trackChunk.Resize(ratio);
        }

        /// <summary>
        /// Resizes <see cref="TrackChunk"/> by the specified ratio. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to resize.</param>
        /// <param name="ratio">Ratio to resize <paramref name="trackChunk"/> by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        public static void Resize(this TrackChunk trackChunk, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            trackChunk.ProcessTimedEvents(timedEvent => timedEvent.Time = MathUtilities.RoundToLong(timedEvent.Time * ratio));
        }

        /// <summary>
        /// Resizes collection of <see cref="TrackChunk"/> to the specified length. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to resize.</param>
        /// <param name="length">New length of the <paramref name="trackChunks"/>.</param>
        /// <param name="tempoMap">Tempo map used to calculate length.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Resize(this IEnumerable<TrackChunk> trackChunks, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!trackChunks.Any(c => c.Events.Any()))
                return;

            var duration = trackChunks.GetDuration<MidiTimeSpan>(tempoMap);
            if (duration.IsZeroTimeSpan())
                return;

            var ratio = GetRatio(duration, length, tempoMap);

            trackChunks.Resize(ratio);
        }

        /// <summary>
        /// Resizes collection of <see cref="TrackChunk"/> by the specified ratio. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to resize.</param>
        /// <param name="ratio">Ratio to resize <paramref name="trackChunks"/> by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        public static void Resize(this IEnumerable<TrackChunk> trackChunks, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.Resize(ratio);
            }
        }

        /// <summary>
        /// Resizes <see cref="MidiFile"/> to the specified length. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to resize.</param>
        /// <param name="length">New length of the <paramref name="midiFile"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Resize(this MidiFile midiFile, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(length), length);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().Resize(length, tempoMap);
        }

        /// <summary>
        /// Resizes <see cref="MidiFile"/> by the specified ratio. More info in the
        /// <see href="xref:a_resizer">Resizer</see> article.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to resize.</param>
        /// <param name="ratio">Ratio to resize <paramref name="midiFile"/> by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        public static void Resize(this MidiFile midiFile, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            midiFile.GetTrackChunks().Resize(ratio);
        }

        private static double GetRatio(MidiTimeSpan duration, ITimeSpan length, TempoMap tempoMap)
        {
            var oldLength = TimeConverter.ConvertTo(duration, length.GetType(), tempoMap);
            return TimeSpanUtilities.Divide(length, oldLength);
        }

        #endregion
    }
}
