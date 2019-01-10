using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Utility methods for <see cref="MidiFile"/>.
    /// </summary>
    public static class MidiFileUtilities
    {
        #region Methods

        /// <summary>
        /// Shifts events forward inside <see cref="MidiFile"/> by the specified distance.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to shift.</param>
        /// <param name="distance">Distance to shift events by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="distance"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public static void Resize(this MidiFile midiFile, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(length), length);

            var lastTime = midiFile.GetTimedEvents().LastOrDefault()?.Time ?? 0;
            if (lastTime == 0)
                return;

            var tempoMap = midiFile.GetTempoMap();

            var oldLength = TimeConverter.ConvertTo((MidiTimeSpan)lastTime, length.GetType(), tempoMap);
            var ratio = TimeSpanUtilities.Divide(length, oldLength);

            ResizeByRatio(midiFile, ratio);
        }

        /// <summary>
        /// Resizes <see cref="MidiFile"/> by the specified ratio.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to resize.</param>
        /// <param name="ratio">Ratio to resize <paramref name="midiFile"/> by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        public static void Resize(this MidiFile midiFile, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");

            ResizeByRatio(midiFile, ratio);
        }

        private static void ResizeByRatio(MidiFile midiFile, double ratio)
        {
            midiFile.ProcessTimedEvents(e => e.Time = MathUtilities.RoundToLong(e.Time * ratio));
        }

        #endregion
    }
}
