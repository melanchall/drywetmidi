using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Utility methods for <see cref="MidiFile"/>.
    /// </summary>
    public static class MidiFileUtilities
    {
        #region Methods

        public static TTimeSpan GetDuration<TTimeSpan>(this TrackChunk trackChunk, TempoMap tempoMap)
            where TTimeSpan : class, ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return trackChunk
                .Events
                .GetTimedEventsLazy(null, false)
                .LastOrDefault()
                ?.TimeAs<TTimeSpan>(tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan<TTimeSpan>();
        }

        public static ITimeSpan GetDuration(this TrackChunk trackChunk, TimeSpanType durationType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsInvalidEnumValue(nameof(durationType), durationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return trackChunk
                .Events
                .GetTimedEventsLazy(null, false)
                .LastOrDefault()
                ?.TimeAs(durationType, tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan(durationType);
        }

        public static TTimeSpan GetDuration<TTimeSpan>(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap)
            where TTimeSpan : class, ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return trackChunks
                .GetTimedEventsLazy(null, false)
                .Select(e => e.Item1)
                .LastOrDefault()
                ?.TimeAs<TTimeSpan>(tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan<TTimeSpan>();
        }

        public static ITimeSpan GetDuration(this IEnumerable<TrackChunk> trackChunks, TimeSpanType durationType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsInvalidEnumValue(nameof(durationType), durationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return trackChunks
                .GetTimedEventsLazy(null, false)
                .Select(e => e.Item1)
                .LastOrDefault()
                ?.TimeAs(durationType, tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan(durationType);
        }

        /// <summary>
        /// Gets the duration of the specified <see cref="MidiFile"/>. Duration is
        /// defined by the time of last MIDI event of the file.
        /// </summary>
        /// <typeparam name="TTimeSpan">The type of time span representing the duration of <paramref name="midiFile"/>.</typeparam>
        /// <param name="midiFile"><see cref="MidiFile"/> to get duration of.</param>
        /// <returns>An instance of <typeparamref name="TTimeSpan"/> representing
        /// duration of <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="TTimeSpan"/> is not supported.</exception>
        public static TTimeSpan GetDuration<TTimeSpan>(this MidiFile midiFile)
            where TTimeSpan : class, ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var tempoMap = midiFile.GetTempoMap();
            return midiFile
                .GetTrackChunks()
                .GetDuration<TTimeSpan>(tempoMap);
        }

        /// <summary>
        /// Gets the duration of the specified <see cref="MidiFile"/>. Duration is
        /// defined by the time of last MIDI event of the file.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to get duration of.</param>
        /// <param name="durationType">The type of time span representing the duration of <paramref name="midiFile"/>.</param>
        /// <returns>An implementation of <see cref="ITimeSpan"/> representing
        /// duration of <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="durationType"/> specified an invalid value.</exception>
        public static ITimeSpan GetDuration(this MidiFile midiFile, TimeSpanType durationType)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsInvalidEnumValue(nameof(durationType), durationType);

            var tempoMap = midiFile.GetTempoMap();
            return midiFile
                .GetTrackChunks()
                .GetDuration(durationType, tempoMap);
        }

        /// <summary>
        /// Checks whether the specified <see cref="MidiFile"/> is empty or not. <see cref="MidiFile"/>
        /// is empty when it doesn't contain MIDI events.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to check emptiness of.</param>
        /// <returns>A value indicating whether <paramref name="midiFile"/> is empty or not.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static bool IsEmpty(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return !midiFile.GetEvents().Any();
        }

        /// <summary>
        /// Shifts events forward inside <see cref="MidiFile"/> by the specified distance.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to shift.</param>
        /// <param name="distance">Distance to shift events by.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ShiftEvents(this MidiFile midiFile, ITimeSpan distance)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);

            midiFile.GetTrackChunks().ShiftEvents(distance, midiFile.GetTempoMap());
        }

        #endregion
    }
}
