using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class BarBeatTimeSpanUtilities
    {
        #region Methods

        public static int GetBarLength(long bars, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timeSignatureAndticksPerQuarterNote = GetTimeSignatureAndTicksPerQuarterNote(bars, tempoMap);
            return GetBarLength(timeSignatureAndticksPerQuarterNote.Item1,
                                timeSignatureAndticksPerQuarterNote.Item2);
        }

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
            var timeSignature = tempoMap.TimeSignature.AtTime(ticks);
            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;

            return Tuple.Create(timeSignature, ticksPerQuarterNote);
        }

        #endregion
    }
}
