using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Utilities for working with bars and beats.
    /// </summary>
    public static class BarBeatUtilities
    {
        #region Methods

        /// <summary>
        /// Gets the length of a bar (in ticks) that is started at distance of the specified bars.
        /// </summary>
        /// <param name="bars">Distance in bars where the bar is started.</param>
        /// <param name="tempoMap">Tempo map used for calculations.</param>
        /// <returns>Length of a bar in ticks.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        public static int GetBarLength(long bars, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timeSignatureAndticksPerQuarterNote = GetTimeSignatureAndTicksPerQuarterNote(bars, tempoMap);
            return GetBarLength(timeSignatureAndticksPerQuarterNote.Item1,
                                timeSignatureAndticksPerQuarterNote.Item2);
        }

        /// <summary>
        /// Gets the length of a beat (in ticks) of the bar that is started at distance of the specified bars.
        /// </summary>
        /// <param name="bars">Distance in bars where the bar is started.</param>
        /// <param name="tempoMap">Tempo map used for calculations.</param>
        /// <returns>Length of a beat in ticks.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        public static int GetBeatLength(long bars, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timeSignatureAndticksPerQuarterNote = GetTimeSignatureAndTicksPerQuarterNote(bars, tempoMap);
            return GetBeatLength(timeSignatureAndticksPerQuarterNote.Item1,
                                 timeSignatureAndticksPerQuarterNote.Item2);
        }

        internal static int GetBarLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            return timeSignature.Numerator * beatLength;
        }

        internal static int GetBeatLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            return 4 * ticksPerQuarterNote / timeSignature.Denominator;
        }

        private static Tuple<TimeSignature, short> GetTimeSignatureAndTicksPerQuarterNote(long bars, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division of the tempo map is not supported.", nameof(tempoMap));

            var ticks = TimeConverter.ConvertFrom(new BarBeatTicksTimeSpan(bars), tempoMap);
            var timeSignature = tempoMap.TimeSignatureLine.GetValueAtTime(ticks);
            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;

            return Tuple.Create(timeSignature, ticksPerQuarterNote);
        }

        #endregion
    }
}
