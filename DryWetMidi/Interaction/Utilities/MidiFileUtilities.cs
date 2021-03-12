using System;
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
                .GetTimedEventsLazy(false)
                .Select(e => e.Item1)
                .LastOrDefault()?.TimeAs<TTimeSpan>(tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan<TTimeSpan>();
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
                .GetTimedEventsLazy(false)
                .Select(e => e.Item1)
                .LastOrDefault()?.TimeAs(durationType, tempoMap) ?? TimeSpanUtilities.GetZeroTimeSpan(durationType);
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

        /// <summary>
        /// Resizes <see cref="MidiFile"/> to the specified length.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to resize.</param>
        /// <param name="length">New length of the <paramref name="midiFile"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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

            if (midiFile.IsEmpty())
                return;

            var tempoMap = midiFile.GetTempoMap();
            var duration = midiFile.GetDuration<MidiTimeSpan>();

            var oldLength = TimeConverter.ConvertTo(duration, length.GetType(), tempoMap);
            var ratio = TimeSpanUtilities.Divide(length, oldLength);

            ResizeByRatio(midiFile, ratio);
        }

        /// <summary>
        /// Resizes <see cref="MidiFile"/> by the specified ratio.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to resize.</param>
        /// <param name="ratio">Ratio to resize <paramref name="midiFile"/> by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        public static void Resize(this MidiFile midiFile, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");

            ResizeByRatio(midiFile, ratio);
        }

        private static void ResizeByRatio(MidiFile midiFile, double ratio)
        {
            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                trackChunk.ProcessTimedEvents(timedEvent => timedEvent.Time = MathUtilities.RoundToLong(timedEvent.Time * ratio));
            }
        }

        #endregion
    }
}
